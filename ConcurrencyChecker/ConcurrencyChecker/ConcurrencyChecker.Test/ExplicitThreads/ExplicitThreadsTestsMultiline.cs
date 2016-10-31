using ConcurrencyChecker.ExplicitThreadsChecker;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace ConcurrencyChecker.Test.ExplicitThreads
{
    [TestClass]
    public class UnitTestMultiline : CodeFixVerifier
    {
        
        [TestMethod]
        public void TestMultilineCodeSmell()
        {
            var test = @"
using System.Threading;

namespace ExplicitThreadsSmell
{
    class SimpleThread
    {
        public void Test1()
        {
            Thread t;
            t = new Thread(Compute);
            t.Start();
        }
    }
}";
            var expected = new DiagnosticResult
            {
                Id = "ETC002",
                Message = "\'t\' should be replaced with Task.Run",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 12, 13)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);

        }

        [TestMethod]
        public void TestMultilineCodeSmellDirectAssignment()
        {
            const string test = @"
using System.Threading;

namespace ExplicitThreadsSmell
{
    class SimpleThread
    {
        public void Test1()
        {
            Thread t = new Thread(Compute);
            t.Start();
        }
    }
}";
            var expected = new DiagnosticResult
            {
                Id = "ETC002",
                Message = "\'t\' should be replaced with Task.Run",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 11, 13)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);

        }

        [TestMethod]
        public void TestMultilineNoSmell()
        {
            const string test = @"
using System.Threading;

namespace ExplicitThreadsSmell
{
    class SimpleThread
    {
        public void Test1()
        {
            Thread t;
            t = new Thread(Compute);
            t.Start();
            t.Join();
        }
    }
}";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void TestMultilineNoSmellMethod()
        {
            const string test = @"
using System.Threading;

namespace ExplicitThreadsSmell
{
    class SimpleThread
    {
        public void Test1()
        {
            Thread t;
            t = new Thread(Compute);
            t.Start();
            Hallo(t);
        }
        private void Hallo(Thread thread)
        {
            throw new NotImplementedException();
        }
    }
}";

            VerifyCSharpDiagnostic(test);
        }


        [TestMethod]
        public void TestMultilineCodeSmellMultiDeclaration()
        {
            var test = @"
using System.Threading;

namespace ExplicitThreadsSmell
{
    class SimpleThread
    {
        public void Test1()
        {
            Thread t,j;
            t = new Thread(Compute);
            j = new Thread(Compute);
            t.Start();
            j.Start();
        }
    }
}";
            var expected1 = new DiagnosticResult
            {
                Id = "ETC002",
                Message = "\'t\' should be replaced with Task.Run",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 13, 13)
                        }
            };

            var expected2 = new DiagnosticResult
            {
                Id = "ETC002",
                Message = "\'j\' should be replaced with Task.Run",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 14, 13)
                        }
            };

            VerifyCSharpDiagnostic(test, expected1, expected2);

        }


        [TestMethod]
        public void TestMultilineCodeSmellMultiDeclarationNoSmell()
        {
            var test = @"
using System.Threading;

namespace ExplicitThreadsSmell
{
    class SimpleThread
    {
        public void Test1()
        {
            Thread t,j;
            t = new Thread(Compute);
            j = new Thread(Compute);
            t.Start();
            j.Start();
            j.Join();
            Compute(t);
        }
    }
}";
            VerifyCSharpDiagnostic(test);

        }


        [TestMethod]
        public void TestThreadCodeFixMultilineDirectInstantition()
        {
            var test = @"
using System.Threading;

namespace ExplicitThreadsSmell
{
    class SimpleThread
    {
        public void Test1()
        {
            Thread t = new Thread(Compute);
            t.Start();
        }
    }
}";
            var expected = new DiagnosticResult
            {
                Id = "ETC002",
                Message = "'t' should be replaced with Task.Run",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 11, 13)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"
using System.Threading;
using System.Threading.Tasks;

namespace ExplicitThreadsSmell
{
    class SimpleThread
    {
        public void Test1()
        {
            Task.Run(() => Compute());
        }
    }
}";
            VerifyCSharpFix(test, fixtest, allowNewCompilerDiagnostics: true);
        }

        [TestMethod]
        public void TestThreadCodeFixMultilineSeparateInstantition()
        {
            var test = @"
using System.Threading;

namespace ExplicitThreadsSmell
{
    class SimpleThread
    {
        public void Test1()
        {
            Thread t;
            t = new Thread(Compute);
            t.Start();
        }
    }
}";
            var expected = new DiagnosticResult
            {
                Id = "ETC002",
                Message = "'t' should be replaced with Task.Run",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 12, 13)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"
using System.Threading;
using System.Threading.Tasks;

namespace ExplicitThreadsSmell
{
    class SimpleThread
    {
        public void Test1()
        {
            Task.Run(() => Compute());
        }
    }
}";
            VerifyCSharpFix(test, fixtest, allowNewCompilerDiagnostics: true);
        }


        [TestMethod]
        public void TestThreadCodeFixMultilineMultiDeclaration()
        {
            var test = @"
using System.Threading;

namespace ExplicitThreadsSmell
{
    class SimpleThread
    {
        public void Test1()
        {
            Thread t,f;
            t = new Thread(Compute);
            t.Start();
        }
    }
}";
            var expected = new DiagnosticResult
            {
                Id = "ETC002",
                Message = "'t' should be replaced with Task.Run",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 12, 13)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"
using System.Threading;
using System.Threading.Tasks;

namespace ExplicitThreadsSmell
{
    class SimpleThread
    {
        public void Test1()
        {
            Thread f;
            Task.Run(() => Compute());
        }
    }
}";
            VerifyCSharpFix(test, fixtest, allowNewCompilerDiagnostics: true);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new ExplicitThreadsMultilineCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new ExplicitThreadsMultilineAnalyzer();
        }



    }




}