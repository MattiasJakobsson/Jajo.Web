using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Text;
using SuperGlue.Configuration;

namespace SuperGlue
{
    public class ProxyCommand : ICommand
    {
        private bool _shouldBeStarted;

        public string Name { get; set; }
        public string AppPath { get; set; }
        public IEnumerable<int> Ports { get; set; }

        public void Execute()
        {
            var basePath = new Uri(Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase));

            var nginxPath = string.Format("{0}{1}", basePath.AbsolutePath, "/Binaries/nginx");

            if (string.IsNullOrEmpty(Name))
                Name = Path.GetFileName(AppPath);

            var nginxExecutable = string.Format("{0}{1}", nginxPath, "/nginx.exe");

            var process = Restart(nginxExecutable, null);

            Console.WriteLine("Nginx proxy started at: {0}", AppPath);

            var key = Console.ReadKey();
            while (key.Key != ConsoleKey.Q)
            {
                if (key.Key != ConsoleKey.R)
                    continue;

                Console.WriteLine();
                Console.WriteLine("Reloading nginx proxy at: {0}", AppPath);

                process = Restart(nginxExecutable, process);

                key = Console.ReadKey();
            }

            _shouldBeStarted = false;

            if (process != null)
                KillProcessAndChildren(process.Id);
        }

        private Process Restart(string path, Process process)
        {
            _shouldBeStarted = false;

            if(process != null)
                KillProcessAndChildren(process.Id);

            var config = ReloadConfig(Path.GetDirectoryName(path));

            return StartNginx(path, config);
        }

        private Process StartNginx(string path, string config)
        {
            _shouldBeStarted = true;

            var startInfo = new ProcessStartInfo(path, string.Format("-c \"{0}\"", config))
            {
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = Path.GetDirectoryName(path),
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true
            };

            var process = new Process
            {
                StartInfo = startInfo,
                EnableRaisingEvents = true
            };

            process.OutputDataReceived += (x, y) => Console.WriteLine(y.Data);

            process.Exited += (x, y) => StartProcess(process);
            process.ErrorDataReceived += (x, y) => Console.WriteLine("Proxy error: {0}", y.Data);

            StartProcess(process);

            return process;
        }

        private void StartProcess(Process process)
        {
            if (!_shouldBeStarted || !process.Start())
                return;

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
        }

        private static void KillProcessAndChildren(int pid)
        {
            var searcher = new ManagementObjectSearcher("Select * From Win32_Process Where ParentProcessID=" + pid);
            var moc = searcher.Get();

            foreach (var mo in moc.Cast<ManagementObject>())
                KillProcessAndChildren(Convert.ToInt32(mo["ProcessID"]));

            try
            {
                var proc = Process.GetProcessById(pid);
                proc.Kill();
            }
            catch (ArgumentException)
            { /* process already exited */ }
        }

        private string ReloadConfig(string nginxPath)
        {
            var applications = FindApplicationsToRun(AppPath);

            var config = new NginxSettings(Ports, applications);

            var configPath = string.Format("{0}\\{1}\\{2}.conf", nginxPath, "config", Name);

            config.WriteToLocation(configPath);

            return configPath;
        }

        private static IEnumerable<HostingOptions> FindApplicationsToRun(string fromLocation)
        {
            var files = Directory.GetFiles(fromLocation, ".hostingoptions");

            foreach (var file in files)
            {
                var options = Files.ReadAsJson<HostingOptions>(file).Result;

                if (options != null)
                    yield return options;
            }

            foreach (var child in Directory.GetDirectories(fromLocation))
            {
                foreach (var option in FindApplicationsToRun(child))
                    yield return option;
            }
        }

        public class HostingOptions
        {
            public HostingOptions()
            {
                Bindings = new List<string>();
            }

            public ICollection<string> Bindings { get; set; }
            public string ApplicationName { get; set; }
            public string ApplicationBasePath { get; set; }
        }

        public class NginxSettings
        {
            private readonly IEnumerable<int> _ports;
            private readonly IEnumerable<HostingOptions> _hosts;

            public NginxSettings(IEnumerable<int> ports, IEnumerable<HostingOptions> hosts)
            {
                _ports = ports;
                _hosts = hosts;
            }

            public void WriteToLocation(string location)
            {
                var directory = Path.GetDirectoryName(location);

                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                var fileBuilder = new StringBuilder();

                fileBuilder.Append("events {\n\tworker_connections  1024;\n}\n");

                fileBuilder.Append("http {\n");

                fileBuilder.Append("\tserver {\n");

                foreach (var port in _ports)
                    fileBuilder.AppendFormat("\t\tlisten {0};\n", port);

                fileBuilder.Append("\t\tserver_name localhost;\n\n");

                foreach (var host in _hosts)
                {
                    var defaultBinding = host.Bindings.FirstOrDefault();

                    if(string.IsNullOrEmpty(defaultBinding))
                        continue;

                    var basePath = host.ApplicationBasePath;

                    if (string.IsNullOrEmpty(basePath))
                        basePath = "/";

                    fileBuilder.AppendFormat("\t\tlocation {0} {{\n", basePath);
                    fileBuilder.AppendFormat("\t\t\tproxy_pass {0};\n", defaultBinding);
                    fileBuilder.Append("\t\t\tproxy_set_header Host $host;\n");
                    fileBuilder.Append("\t\t\tproxy_set_header X-Real-IP $remote_addr;\n");
                    fileBuilder.AppendFormat("\t\t\tproxy_cookie_domain {0} $host;\n", defaultBinding);
                    fileBuilder.Append("\t\t\tproxy_http_version 1.1;\n");
                    fileBuilder.Append("\t\t}\n\n");
                }

                fileBuilder.Append("\t}\n}");

                File.WriteAllText(location, fileBuilder.ToString());
            }
        }
    }
}