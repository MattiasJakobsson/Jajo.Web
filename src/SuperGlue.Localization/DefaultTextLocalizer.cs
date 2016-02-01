using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using SuperGlue.Configuration;
using SuperGlue.FileSystem;

namespace SuperGlue.Localization
{
    public class DefaultTextLocalizer : ILocalizeText
    {
        private static readonly IDictionary<string, string> Translations = new ConcurrentDictionary<string, string>();
        private static readonly object MissingLocker = new object();
        public const string MissingLocaleConfigFile = "missing.locale.config";
        private const string LeafElement = "string";
        private readonly IEnumerable<ILocalizationVisitor> _visitors;
        private readonly IDictionary<string, object> _environment;
        private readonly IFileSystem _fileSystem;

        public DefaultTextLocalizer(IEnumerable<ILocalizationVisitor> visitors, IDictionary<string, object> environment, IFileSystem fileSystem)
        {
            _visitors = visitors;
            _environment = environment;
            _fileSystem = fileSystem;
        }

        public virtual async Task<string> Localize(string key, CultureInfo culture)
        {
            var translationKey = key;
            var writeMissing = true;
            var missingKeys = new List<string>();

            while (true)
            {
                var translationResult = await GetTranslation(translationKey, culture).ConfigureAwait(false);

                if (translationResult.Item2)
                    return _visitors.Aggregate(translationResult.Item1, (current, visitor) => visitor.AfterLocalized(translationKey, current));

                if (writeMissing)
                    missingKeys.Add(translationKey);

                var keyParts = translationKey.Split(':');

                var text = translationKey;

                if (keyParts.Length <= 1)
                {
                    WriteMissing(missingKeys, culture);

                    return text;
                }

                var namespaceParts = keyParts.Take(keyParts.Length - 1).ToArray();

                if (namespaceParts.Length <= 0)
                {
                    WriteMissing(missingKeys, culture);

                    return text;
                }

                var namespacePartsToUse = namespaceParts.Take(namespaceParts.Length - 1).ToArray();
                translationKey = keyParts.Last();

                if (namespacePartsToUse.Any())
                    translationKey = $"{string.Join(":", namespacePartsToUse)}:{translationKey}";

                writeMissing = false;
            }
        }

        public virtual Task Load()
        {
            var fileSet = new FileSet
            {
                DeepSearch = false,
                Include = "*.locale.config",
                Exclude = MissingLocaleConfigFile
            };

            var directories = GetDirectoriesToSearch();

            var files = directories.SelectMany(dir => _fileSystem.FindFiles(dir, fileSet)).Where(file =>
            {
                var fileName = Path.GetFileName(file);
                return fileName != null && !fileName.StartsWith("missing.");
            }).GroupBy(CultureFor);

            foreach (var file in files)
            {
                var items = file.SelectMany(LoadFrom);

                foreach (var item in items)
                {
                    var key = BuildKey(file.Key, item.Item1);

                    Translations[key] = item.Item2;
                }
            }

            return Task.CompletedTask;
        }

        protected virtual Task<Tuple<string, bool>> GetTranslation(string key, CultureInfo culture)
        {
            var translationKey = BuildKey(culture, key);

            return Task.FromResult(Translations.ContainsKey(translationKey) ? new Tuple<string, bool>(Translations[translationKey], true) : new Tuple<string, bool>("", false));
        }

        protected virtual string GetDefaultText(string key, CultureInfo culture)
        {
            return key;
        }

        protected virtual void WriteMissing(IEnumerable<string> keys, CultureInfo culture)
        {
            foreach (var key in keys)
                WriteMissing(key, culture);
        }

        protected virtual void WriteMissing(string key, CultureInfo culture)
        {
            var missingFileLocation = GetMissingLocaleFileLocation();

            lock (MissingLocker)
            {
                var missingDocument = GetMissingKeysDocument(missingFileLocation);

                if (missingDocument.DocumentElement?.SelectSingleNode($"missing[@key='{key}']") != null)
                    return;

                missingDocument.DocumentElement.AddElement("missing").WithAtt("key", key).WithAtt("culture", culture.Name).InnerText = "";
                missingDocument.Save(missingFileLocation);
            }
        }

        protected virtual IEnumerable<string> GetDirectoriesToSearch()
        {
            yield return _environment.ResolvePath("~/");
        }

        private static CultureInfo CultureFor(string filename)
        {
            var fileName = Path.GetFileName(filename);

            return fileName != null ? new CultureInfo(fileName.Split('.').First()) : null;
        }

        private static XmlDocument GetMissingKeysDocument(string fileLocation)
        {
            return fileLocation.XmlFromFileWithRoot("missing-localization");
        }

        private string GetMissingLocaleFileLocation()
        {
            var directory = GetDirectoriesToSearch().FirstOrDefault();

            if (string.IsNullOrEmpty(directory))
                return null;

            return directory.AppendPath(MissingLocaleConfigFile);
        }

        private static IEnumerable<Tuple<string, string>> LoadFrom(string file)
        {
            var document = file.XmlFromFileWithRoot("sg-localization");

            var xmlNodeList = document.DocumentElement?.SelectNodes(LeafElement);

            if (xmlNodeList == null)
                yield break;

            foreach (XmlElement element in xmlNodeList)
                yield return new Tuple<string, string>(element.GetAttribute("key"), element.InnerText);
        }

        private static string BuildKey(CultureInfo culture, string key)
        {
            return $"{culture.Name.ToLower()}-{key}";
        }
    }
}