using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NAntAsync;

namespace TestNAntAsync
{
    [TestClass]
    public class IntegrationTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            new AsyncTask();
            NAnt.Assert("AsyncCallingTarget.build");
        }
    }
}
