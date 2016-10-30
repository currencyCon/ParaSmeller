using ConcurrencyChecker.PrimitiveSynchronizationChecker;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace ConcurrencyChecker.Test.PrimitiveSynchronization
{
    [TestClass]
    public class PrimitiveSynchronizationTests: CodeFixVerifier
    {
        [TestMethod]
        public void AnalyzerFindsInterlockedSimpleCase()
        {
            var test = @"
using System.Threading;

namespace Test
{
    public class BancAcount
    {
        private int _balance;

        public void Withdraw(int amount)
        {
            var value = _balance;
            if (value >= _balance)
            {
                Interlocked.Add(ref _balance, -amount);
            }
        }
    }
}";
            var expected = new DiagnosticResult
            {
                Id = PrimitiveSynchronizationAnalyzer.InterlockedUsageDiagnosticId,
                Message = PrimitiveSynchronizationAnalyzer.MessageFormatInterlockedUsage.ToString(),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 15, 17)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);

        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new PrimitiveSynchronizationAnalyzer();
        }
    }
}
