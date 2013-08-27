using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NAntAsync;

namespace TestNAntAsync
{
    [TestClass]
    public class IntegrationTest
    {
        private AsyncTask _keepNAntAsyncTasksDllDeployed = new AsyncTask();

        [TestMethod]
        public void AsyncCallingTarget()
        {
            NAnt.Assert("AsyncCallingTarget.build");
        }

        [TestMethod]
        public void AsyncStartProcess()
        {
            NAnt.Assert("AsyncStartProcess.build");
        }

        [TestMethod]
        public void AsyncStartProcessFailOnError()
        {
            NAnt.Assert("AsyncStartProcessFailOnError.build");
        }
    }
}
