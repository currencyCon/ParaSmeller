
using System;
using System.Threading.Tasks;
using ConcurrencyChecker.FireAndForgetChecker;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace ConcurrencyChecker.Test.FireAndForget
{
    [TestClass]
    public class FireAndForgetCheckerTests: CodeFixVerifier
    {
        [TestMethod]
        public void TestFindsSimpleCase()
        {
            const string test = @"namespace Test
{
    public class SimpleTest
    {
        public static void X()
        {
            Thread.Sleep(500000000);
            Console.WriteLine(""Hello"");
        }
        public static void Main()
        {
            var x = Task.Run(() => X());
            Console.WriteLine(""Lol"");
        }
    }
}
";
            var x = Task.Run(() =>
            {
                Console.WriteLine("Hello");
            });
            var expected = new DiagnosticResult
            {
                Id = FireAndForgetCheckerAnalyzer.FireAndForgetCallId,
                Message = "The result of this Computation is potentially never awaited",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] {
                    new DiagnosticResultLocation("Test0.cs", 7, 25)
                }
            };

            VerifyCSharpDiagnostic(test, expected);

        }



        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new FireAndForgetCheckerAnalyzer();
        }
    }
}
