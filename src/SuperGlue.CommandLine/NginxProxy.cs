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
    public class NginxProxy
    {
        private bool _shouldBeStarted;
        private readonly IEnumerable<string> _applicationPaths;
        private readonly int _port;
        private readonly string _name;
        private Process _process;

        public NginxProxy(IEnumerable<string> applicationPaths, int port, string name)
        {
            _applicationPaths = applicationPaths;
            _port = port;
            _name = name;
        }

        public void Start()
        {
            var basePath = new Uri(Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase));

            var nginxPath = string.Format("{0}{1}", basePath.AbsolutePath, "/Binaries/nginx");

            var nginxExecutable = string.Format("{0}{1}", nginxPath, "/nginx.exe");

            Restart(nginxExecutable);
        }

        public void Stop()
        {
            _shouldBeStarted = false;

            if (_process != null)
                KillProcessAndChildren(_process.Id);
        }

        private void Restart(string path)
        {
            _shouldBeStarted = false;

            if (_process != null)
                KillProcessAndChildren(_process.Id);

            var config = ReloadConfig(Path.GetDirectoryName(path));

            StartNginx(path, config);
        }

        private void StartNginx(string path, string config)
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

            _process = new Process
            {
                StartInfo = startInfo,
                EnableRaisingEvents = true
            };

            _process.OutputDataReceived += (x, y) => Console.WriteLine(y.Data);

            _process.Exited += (x, y) => StartProcess();
            _process.ErrorDataReceived += (x, y) => Console.WriteLine("Proxy error: {0}", y.Data);

            StartProcess();
        }

        private void StartProcess()
        {
            if (!_shouldBeStarted || !_process.Start())
                return;

            _process.BeginOutputReadLine();
            _process.BeginErrorReadLine();
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
            var applications = FindApplicationsToRun().ToList();

            var config = new NginxSettings(_port, applications);

            var configPath = string.Format("{0}\\{1}\\{2}.conf", nginxPath, "config", _name);

            config.WriteToLocation(configPath);

            return configPath;
        }

        private IEnumerable<HostingOptions> FindApplicationsToRun()
        {
            return _applicationPaths
                .Select(applicationPath => Path.Combine(applicationPath, ".hostingoptions"))
                .Where(File.Exists)
                .Select(file => Files.ReadAsJson<HostingOptions>(file).Result)
                .Where(options => options != null);
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
            private readonly int _port;
            private readonly IEnumerable<HostingOptions> _hosts;

            public NginxSettings(int port, IEnumerable<HostingOptions> hosts)
            {
                _port = port;
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

                fileBuilder.AppendFormat("\t\tlisten {0};\n", _port);

                fileBuilder.Append("\t\tserver_name localhost;\n\n");

                foreach (var host in _hosts)
                {
                    var defaultBinding = host.Bindings.FirstOrDefault();

                    if (string.IsNullOrEmpty(defaultBinding))
                        continue;

                    var basePath = host.ApplicationBasePath;

                    if (string.IsNullOrEmpty(basePath))
                        basePath = "/";

                    fileBuilder.AppendFormat("\t\tlocation {0} {{\n", basePath);
                    fileBuilder.AppendFormat("\t\t\tproxy_pass {0};\n", defaultBinding);
                    fileBuilder.Append("\t\t\tproxy_set_header Host $host:$server_port;\n");
                    fileBuilder.Append("\t\t\tproxy_set_header X-Real-IP $remote_addr;\n");
                    fileBuilder.Append("\t\t\tproxy_set_header X-Forwarded-For $remote_addr;\n");
                    fileBuilder.AppendFormat("\t\t\tproxy_cookie_domain {0} $host;\n", defaultBinding);
                    fileBuilder.Append("\t\t\tproxy_http_version 1.1;\n");
                    fileBuilder.Append("\t\t\tproxy_connect_timeout 10s;\n");
                    fileBuilder.Append("\t\t}\n\n");
                }

                fileBuilder.Append("\t}\n}");

                File.WriteAllText(location, fileBuilder.ToString());
            }
        }
    }
}