using ConcurrencyChecker.MonitorWaitOrSignal;
using ConcurrencyChecker.OverAsynchrony;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace ConcurrencyChecker.Test.OverAsynchrony
{
    [TestClass]
    public class OverAsynchronyUnitTests : CodeFixVerifier
    {
        [TestMethod]
        public void TestNoDiagnostics()
        {
            const string test = @"
using System.Threading;
using System.Threading.Tasks;

namespace OverAsynchrony
{
    class Service
    {
        public async void Sleep()
        {
            await Task.Run(() => { Thread.Sleep(1000); });
        }
    }
}";
            VerifyCSharpDiagnostic(test);
        }


        [TestMethod]
        public void TestSimpleIfDiagnostics()
        {
            const string test = @"
using System.Threading;
using System.Threading.Tasks;

namespace OverAsynchrony
{
    class Service
    {
        private async void Sleep()
        {
            await Task.Run(() => { Thread.Sleep(1000); });
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = "OA001",
                Message = "async shouldn't be used in private methods",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 9, 9)
                    }
            };
            
            VerifyCSharpDiagnostic(test, expected);
        }
        

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return null;
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new OverAsynchronyAnalyzer();
        }
    }
}