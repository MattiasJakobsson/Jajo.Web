using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace OpenWeb.Output.Spark
{
    public abstract class BaseTemplateSource : ITemplateSource
    {
        private const string ViewModelTypePattern = @"<viewdata model=""([a-z&auml;&aring;&ouml;A-Z&Auml;&Aring;&Ouml;0-9!&amp;%/\(\)=\?;:\-_ \t\.\,]*)""";

        public abstract IEnumerable<Template> FindTemplates();

        protected static Type FindModelType(TextReader contentReader, IEnumerable<Assembly> availableAssemblies)
        {
            var content = contentReader.ReadToEnd();

            var match = Regex.Match(content, ViewModelTypePattern);

            if (!match.Success) return null;

            var captured = match.Captures.OfType<Capture>().FirstOrDefault();

            if (captured == null) return null;

            var capturedString = captured.Value.Replace("<viewdata model=\"", string.Empty).Replace("\"", string.Empty);

            var assembly = availableAssemblies.FirstOrDefault(x => x.GetType(capturedString, false) != null);

            if (assembly == null) return null;

            return assembly.GetType(capturedString, true);
        }
    }
}