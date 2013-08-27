using System.Threading.Tasks;
using NAnt.Core;
using NAnt.Core.Attributes;

namespace NAntAsync
{
    [TaskName("async")]
    public class AsyncTask : TaskContainer
    {
        [TaskAttribute("target")]
        public string Target { get; set; }

        protected override void ExecuteTask()
        {
            var asyncTask = GetAsyncTask();
            asyncTask.Start();
            base.ExecuteTask();
            asyncTask.Wait();
        }

        private System.Threading.Tasks.Task GetAsyncTask()
        {
            Target target = null;
            if (!string.IsNullOrWhiteSpace(Target))
                target = Project.Targets.Find(Target);
            if (target != null)
                return new System.Threading.Tasks.Task(() => target.Execute());
            return new System.Threading.Tasks.Task(() => { });
        }
    }

}
