using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Web.XmlTransform;

namespace SuperGlue
{
    public class RunnableApplication
    {
        private readonly IApplicationRunner _runner;
        private readonly ApplicationsConfiguration.ApplicationConfigurationDetails _application;
        private readonly string _environment;
        private readonly ICollection<FileListener> _fileListeners = new List<FileListener>();

        public RunnableApplication(IApplicationRunner runner, ApplicationsConfiguration.ApplicationConfigurationDetails application, string environment)
        {
            _runner = runner;
            _application = application;
            _environment = environment;
        }

        public string GetApplicationPath()
        {
            return _application.Destination;
        }

        public async Task Start(string basePath)
        {
            StopListeners();

            if (Directory.Exists(_application.Destination))
                new DirectoryInfo(_application.Destination).DeleteDirectoryAndChildren();

            DirectoryCopy(_application.Path, _application.Destination);

            TransformConfigurationsIn(_application.Destination, ".config", _environment);
            TransformConfigurationsIn(_application.Destination, ".xml", _environment);

            await _runner.Start(_environment);

            var listener = new FileListener();

            listener.StartListening(_application.Path, "*", x => Recycle().Wait());

            _fileListeners.Add(listener);
        }

        public Task Stop()
        {
            StopListeners();

            return _runner.Stop();
        }

        public Task Recycle()
        {
            return _runner.Recycle(_environment);
        }

        private void StopListeners()
        {
            foreach (var fileListener in _fileListeners)
                fileListener.StopListeners();

            _fileListeners.Clear();
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName)
        {
            var dir = new DirectoryInfo(sourceDirName);
            var dirs = dir.GetDirectories();

            if (!dir.Exists)
                throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDirName);

            if (!Directory.Exists(destDirName))
                Directory.CreateDirectory(destDirName);

            var files = dir.GetFiles();
            foreach (var file in files)
            {
                var temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            foreach (var subdir in dirs)
            {
                var temppath = Path.Combine(destDirName, subdir.Name);
                DirectoryCopy(subdir.FullName, temppath);
            }
        }

        private void TransformConfigurationsIn(string directory, string configExtension, string transformation)
        {
            var configFiles = Directory.GetFiles(directory, string.Format("*{0}", configExtension)).ToList();

            var transformationFiles = new List<string>();

            foreach (var configFile in configFiles)
            {
                var fileName = Path.GetFileNameWithoutExtension(configFile);

                if(string.IsNullOrEmpty(fileName))
                    continue;

                transformationFiles.AddRange(configFiles.Where(x => x.StartsWith(fileName) && Path.GetFileNameWithoutExtension(x) != fileName));
            }

            var filesToTransform = configFiles.Except(transformationFiles);

            foreach (var file in filesToTransform)
            {
                var transformationFile = string.Format("{0}.{1}{2}", file, transformation, configExtension);

                if (File.Exists(transformationFile))
                    TransformConfig(string.Format("{0}{1}", file, configExtension), transformationFile);
            }

            foreach (var child in Directory.GetDirectories(directory))
                TransformConfigurationsIn(child, configExtension, transformation);
        }

        private static void TransformConfig(string configFileName, string transformFileName)
        {
            var document = new XmlTransformableDocument
            {
                PreserveWhitespace = true
            };

            document.Load(configFileName);

            var transformation = new XmlTransformation(transformFileName);

            if (!transformation.Apply(document))
                throw new Exception("Transformation Failed");

            document.Save(configFileName);
        }
    }
}