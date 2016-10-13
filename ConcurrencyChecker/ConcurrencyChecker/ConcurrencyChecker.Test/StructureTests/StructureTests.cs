using System;
using ConcurrencyChecker.StructureTests;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace ConcurrencyChecker.Test.StructureTests
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

        private int _a;
        public int AProperty
        {
            get
            {
                lock (this)
                {
                    return _a;
                }
            }
            set
            {
                lock (this)
                {
                    _a = value + B.BProperty;
                }
            }
        }
        public void DoAStuff()
        {
            lock (this)
            {
                B.DoBStuff();
            }
        }
    }

    public class B
    {
        public A A { get; set; }

        private int _b;
        public int BProperty
        {
            get
            {
                lock (this)
                {
                    return _b;
                }
            }
            set
            {
                lock (this)
                {
                    _b = value + A.AProperty;
                }
            }
        }


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
            var x = a.AProperty;
            a.DoAStuff();
        }
    }
}

";
            var expected = new DiagnosticResult
            {
                Id = "ConcurrencyChecker",
                Message = String.Format("Type name '{0}' contains lowercase letters", "Maiclass"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 69, 18)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new ConcurrencyCheckerAnalyzer();
        }
    }
}