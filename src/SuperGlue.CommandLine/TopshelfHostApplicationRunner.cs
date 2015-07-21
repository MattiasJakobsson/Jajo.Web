using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading.Tasks;

namespace SuperGlue
{
    public class TopshelfHostApplicationRunner : IApplicationRunner
    {
        private readonly string _applicationPath;
        private bool _shouldBeStarted;
        private Process _process;

        public TopshelfHostApplicationRunner(string applicationPath)
        {
            _applicationPath = applicationPath;
        }

        public static bool CanRun(string applicationPath)
        {
            return File.Exists(Path.Combine(applicationPath, "SuperGlue.Hosting.Topshelf.exe"));
        }

        public Task Start(string environment)
        {
            _shouldBeStarted = true;

            var path = Path.Combine(_applicationPath, "SuperGlue.Hosting.Topshelf.exe");

            var startInfo = new ProcessStartInfo(path, string.Format("-environment={0}", environment))
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
            _process.ErrorDataReceived += (x, y) => Console.WriteLine("Application error: {0}", y.Data);

            StartProcess();

            return Task.CompletedTask;
        }

        public Task Stop()
        {
            _shouldBeStarted = false;

            if (_process != null)
                KillProcessAndChildren(_process.Id);

            return Task.CompletedTask;
        }

        public async Task Recycle(string environment)
        {
            await Stop();
            await Start(environment);
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

    }
}