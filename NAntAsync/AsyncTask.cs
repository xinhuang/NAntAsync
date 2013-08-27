using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using NAnt.Core;
using NAnt.Core.Attributes;
using Task = System.Threading.Tasks.Task;

namespace NAntAsync
{
    [TaskName("async")]
    public class AsyncTask : TaskContainer
    {
        private string _target;
        private string _exec;

        [TaskAttribute("target")]
        public string Target
        {
            get { return _target; }
            set
            {
                _target = value;
                if (!string.IsNullOrWhiteSpace(Exec))
                    throw new NotSupportedException("Asynchronously execute target and executable at same time is not supported.");
            }
        }

        [TaskAttribute("exec")]
        public string Exec
        {
            get { return _exec; }
            set
            {
                _exec = value;
                if (!string.IsNullOrWhiteSpace(Target))
                    throw new NotSupportedException("Asynchronously execute target and executable at same time is not supported.");
            }
        }

        [TaskAttribute("commandline")]
        public string CommandLine { get; set; }

        [TaskAttribute("output")]
        public string Output { get; set; }

        [TaskAttribute("append")]
        public bool Append { get; set; }

        [TaskAttribute("resultproperty")]
        public string ResultProperty { get; set; }

        [TaskAttribute("workingdir")]
        public string WorkingDir { get; set; }

        [TaskAttribute("basedir")]
        public string BaseDir { get; set; }

        protected override void ExecuteTask()
        {
            var asyncTask = StartAsyncTask();
            base.ExecuteTask();
            try
            {
                asyncTask.Wait();
                if (!string.IsNullOrWhiteSpace(ResultProperty))
                    Project.Properties[ResultProperty] = asyncTask.Result.ToString();
            }
            catch (Exception)
            {
                if (FailOnError)
                    throw;
            }
        }

        private Task<int> StartAsyncTask()
        {
            Task<int> task;
            if (TryStartTargetAsync(out task))
                return task;
            if (TryStartProcessAsync(out task))
                return task;
            throw new ArgumentException("At least an asynchronous target or executable is expected.");
        }

        private bool TryStartProcessAsync(out Task<int> task)
        {
            task = null;
            if (string.IsNullOrWhiteSpace(Exec))
                return false;

            var process = new Process
            {
                StartInfo =
                {
                    FileName = GetImagePath(BaseDir, Exec),
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    Arguments = CommandLine,
                    WorkingDirectory = WorkingDir,
                },
                EnableRaisingEvents = true,
            };
            var source = new TaskCompletionSource<int>();
            process.Exited += (sender, e) => OnAsyncProcessExited(sender, e, source);
            try
            {
                process.Start();
                task = source.Task;
            }
            catch (Exception)
            {
                if (FailOnError)
                    throw;
            }
            return true;
        }

        private string GetImagePath(string baseDir, string exec)
        {
            if (string.IsNullOrWhiteSpace(baseDir))
                return exec;
            return Path.Combine(baseDir, exec);
        }

        private void OnAsyncProcessExited(object sender, EventArgs e, TaskCompletionSource<int> source)
        {
            var process = sender as Process;
            Debug.Assert(process != null);
            source.SetResult(process.ExitCode);
            if (string.IsNullOrWhiteSpace(Output))
                return;
            if (!Append)
            {
                File.WriteAllText(Output, process.StandardOutput.ReadToEnd());
            }
            else
            {
                using (var ostream = File.AppendText(Output))
                {
                    ostream.Write(process.StandardOutput.ReadToEnd());
                }
            }
        }

        private bool TryStartTargetAsync(out Task<int> task)
        {
            task = null;
            if (string.IsNullOrWhiteSpace(Target))
                return false;
            Target target = Project.Targets.Find(Target);
            if (target == null)
                throw new ArgumentException("Cannot find target named `" + Target + "'.");

            task = new Task<int>(() =>
            {
                target.Execute();
                return 0;
            });
            task.Start();
            return true;
        }
    }

}
