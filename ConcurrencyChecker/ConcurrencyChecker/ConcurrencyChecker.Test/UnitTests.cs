using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;

namespace ConcurrencyChecker.Test
{
    [TestClass]
    public class UnitTest : CodeFixVerifier
    {

        //No diagnostics expected to show up
        [TestMethod]
        public void TestMethod1()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public void TestMethod2()
        {
            var test = @"
   namespace ConcurrencyAnalyzer
{
    public class A
    {
        public B B { get; set; }

        public void DoAStuff()
        {
            var x = 2;
            {
                var z = 2;
            }
            lock (this)
            {
                B.DoBStuff();
            }
        }
    }

    public class B
    {
        public A A { get; set; }

        public void DoBStuff()
        {
            lock (this)
            {
                A.DoAStuff();
            }
        }

    }

    public class Maiclass
    {
        public static void Main()
        {
            var a = new A();
            var b = new B();
            a.B = b;
            b.A = a;
            a.DoAStuff();
        }
    }
}
";
            var expected = new DiagnosticResult
            {
                Id = "ConcurrencyChecker",
                Message = String.Format("Type name '{0}' contains lowercase letters", "TypeName"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 11, 15)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new ConcurrencyCheckerCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new ConcurrencyCheckerAnalyzer();
        }
    }
}