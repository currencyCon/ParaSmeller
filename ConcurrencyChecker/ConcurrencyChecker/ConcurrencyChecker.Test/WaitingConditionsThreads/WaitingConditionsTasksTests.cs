using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParaSmellerCore.Reporters;
using TestHelper;

namespace ParaSmeller.Test.WaitingConditionsThreads
{
    [TestClass]
    public class WaitingConditionsTasksTests: CodeFixVerifier
    {
        [TestMethod]
        public void FindsMonitorLockInThreadTest()
        {
            const string test = @"using System.Threading.Tasks;

namespace ConcurrencyChecker.Test.TestCodeTester
    {
        public class SynchronizedThread
        {
            public static object LockObject = new object();
            public static void DoTask()
            {
                lock (LockObject)
                {
                    var c = 2;
                }
            }

            public static void Main()
            {
                Task.Run(() => DoTask());
            }
        }
    }";
            var expected = new DiagnosticResult
            {
                Id = WaitingConditionsTasksReporter.WaitingConditionsTasksDiagnosticId,
                Message = WaitingConditionsTasksReporter.MessageFormat.ToString(),
                Severity = DiagnosticSeverity.Warning,
                Locations =
        new[] {
                            new DiagnosticResultLocation("Test0.cs", 18, 32)
            }
            };

            VerifyCSharpDiagnostic(test, expected);
        }
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new WaitingConditionsTasksAnalyzer();
        }
    }
}
