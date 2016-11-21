using ConcurrencyAnalyzer.Reporters;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace ConcurrencyChecker.Test.ExplicitThreads
{
    [TestClass]
    public class UnitTest : CodeFixVerifier
    {

        [TestMethod]
        public void TestThreadDiagnostics()
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
            var expected1 = new DiagnosticResult
            {
                Id = ExplicitThreadsReporter.DiagnosticId,
                Message = ExplicitThreadsReporter.MessageFormat.ToString(),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 10, 13)
                        }
            };

            var expected2 = new DiagnosticResult
            {
                Id = ExplicitThreadsReporter.DiagnosticId,
                Message = ExplicitThreadsReporter.MessageFormat.ToString(),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 10, 24)
                        }
            };

            VerifyCSharpDiagnostic(test, expected1, expected2);
        }

        [TestMethod]
        public void TestCurrentThread()
        {
            const string test = @"
using System.Threading;

namespace ExplicitThreadsSmell
{
    class SimpleThread
    {
        public void Test1()
        {
            CultureInfo ci = new CultureInfo("");
            Thread.CurrentThread.CurrentCulture = ci;
        }
    }
}";         
            VerifyCSharpDiagnostic(test);
        }


        [TestMethod]
        public void TestMultiDiagnostics()
        {
            const string test = @"
using System.Threading;

namespace ExplicitThreadsSmell
{
    class SimpleThread
    {
        public void Test1()
        {
            while(true) {
                int i = 0;
                if(i == 10)             
                {
                    new Thread(Compute).Start();  
                }
                else 
                {
                    i++;
                    new Thread(Compute).Start();  
                }
            }
        }
    }
}";

            var expected1 = new DiagnosticResult
            {
                Id = ExplicitThreadsReporter.DiagnosticId,
                Message = ExplicitThreadsReporter.MessageFormat.ToString(),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 14, 21),
                        }
            };

            var expected2 = new DiagnosticResult
            {
                Id = ExplicitThreadsReporter.DiagnosticId,
                Message = ExplicitThreadsReporter.MessageFormat.ToString(),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 19, 21),
                        }
            };

            VerifyCSharpDiagnostic(test, expected1, expected2);
        }


        [TestMethod]
        public void TestThreadCodeSmellMethod()
        {
            const string test = @"
using System.Threading;

namespace ExplicitThreadsSmell
{
    class SimpleThread
    {
        public void Test1()
        {
            new Thread(Compute).Start();  
        }
    }
}";
            var expected = new DiagnosticResult
            {
                Id = ExplicitThreadsReporter.DiagnosticId,
                Message = ExplicitThreadsReporter.MessageFormat.ToString(),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 10, 13)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
            
        }

        [TestMethod]
        public void TestThreadCodeSmellLambda()
        {
            const string test = @"
using System.Threading;

namespace ExplicitThreadsSmell
{
    class SimpleThread
    {
        public void Test1()
        {
            new Thread(() => { int i = 10; i++;}).Start();
        }
    }
}";
            var expected = new DiagnosticResult
            {
                Id = ExplicitThreadsReporter.DiagnosticId,
                Message = ExplicitThreadsReporter.MessageFormat.ToString(),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 10, 13)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
            
        }

        [TestMethod]
        public void TestMultilineCodeSmell()
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
        }
    }
}";
            var expected1 = new DiagnosticResult
            {
                Id = ExplicitThreadsReporter.DiagnosticId,
                Message = ExplicitThreadsReporter.MessageFormat.ToString(),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 10, 13)
                        }
            };

            var expected2 = new DiagnosticResult
            {
                Id = ExplicitThreadsReporter.DiagnosticId,
                Message = ExplicitThreadsReporter.MessageFormat.ToString(),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 11, 17)
                        }
            };

            VerifyCSharpDiagnostic(test, expected1, expected2);

        }
        
        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return null;
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new ExplicitThreadsAnalyzer();
        }

        
    }




}