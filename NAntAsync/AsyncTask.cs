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

        protected override void ExecuteTask()
        {
            var asyncTask = StartAsyncTask();
            base.ExecuteTask();
            try
            {
                asyncTask.Wait();
            }
            catch (Exception)
            {
                if (FailOnError)
                    throw;
            }
        }

        private Task StartAsyncTask()
        {
            Task task;
            if (TryStartTargetAsync(out task))
                return task;
            if (TryStartProcessAsync(out task))
                return task;
            throw new ArgumentException("At least an asynchronous target or executable is expected.");
        }

        private bool TryStartProcessAsync(out Task task)
        {
            task = null;
            if (string.IsNullOrWhiteSpace(Exec))
                return false;

            var process = new Process
            {
                StartInfo =
                {
                    FileName = Exec,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    Arguments = CommandLine,
                    WorkingDirectory = Environment.CurrentDirectory,
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

        private void OnAsyncProcessExited(object sender, EventArgs e, TaskCompletionSource<int> source)
        {
            var process = sender as Process;
            Debug.Assert(process != null);
            source.SetResult(process.ExitCode);
            if (string.IsNullOrWhiteSpace(Output))
                return;
            File.WriteAllText(Output, process.StandardOutput.ReadToEnd());
        }

        private bool TryStartTargetAsync(out Task task)
        {
            task = null;
            Target target = null;
            if (!string.IsNullOrWhiteSpace(Target))
                target = Project.Targets.Find(Target);
            if (target == null)
                return false;

            task = new Task(() => target.Execute());
            task.Start();
            return true;
        }
    }

}
