using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestNAntAsync
{
    static class NAnt
    {
        private struct ExecuteResult
        {
            private readonly int _exitCode;
            private readonly string _stdOut;

            public ExecuteResult(int exitCode, string stdOut)
            {
                _exitCode = exitCode;
                _stdOut = stdOut;
            }

            public int ExitCode
            {
                get { return _exitCode; }
            }

            public string StdOut
            {
                get { return _stdOut; }
            }
        }

        public static void AssertFail(string script)
        {
            AssertExitCode(1, script);
        }

        public static void Assert(string script)
        {
            AssertExitCode(0, script);
        }

        private static void AssertExitCode(int expected, string script)
        {
            var result = Execute(script);

            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(expected, result.ExitCode,
                                                                         result.StdOut);
        }

        private static ExecuteResult Execute(string script)
        {
            var process = new Process
            {
                StartInfo =
                {
                    FileName = "nant.exe",
                    Arguments = string.Format(@"-nologo /f:{0} -v", script),
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    WorkingDirectory = Environment.CurrentDirectory
                }
            };

            process.Start();
            process.WaitForExit();

            return new ExecuteResult(process.ExitCode, process.StandardOutput.ReadToEnd());
        }
    }
}