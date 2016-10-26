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
        public void TestPrivateAsyncTest()
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


        [TestMethod]
        public void TestAsyncDepth()
        {
            const string test = @"
using System.Threading;
using System.Threading.Tasks;

namespace OverAsynchrony
{
    class Service
    {
        public async void Test1()
        {
            await Test2();
        }

        public async void Test2()
        {
            await Test3();
        }

        public async void Test3()
        {
            await Task.Run(() => { Thread.Sleep(1000); });
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = "OA002",
                Message = "Async shoudn't be nested 3 times",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 9, 9)
                    }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void TestAsyncDepthGood()
        {
            const string test = @"
using System.Threading;
using System.Threading.Tasks;

namespace OverAsynchrony
{
    class Service
    {
        public async void Test1()
        {
            await Test2();
        }

        public async void Test2()
        {
            await Task.Run(() => { Thread.Sleep(1000); });
        }
    }
}";
            
            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void TestAsyncDepthMultiple()
        {
            const string test = @"
using System.Threading;
using System.Threading.Tasks;

namespace OverAsynchrony
{
    class Service
    {
        public async void Test0()
        {
            await Test1();
        }

        public async void Test1()
        {
            await Test2();
        }

        public async void Test2()
        {
            await Test3();
        }

        public async void Test3()
        {
            await Task.Run(() => { Thread.Sleep(1000); });
        }
    }
}";

            var expected1 = new DiagnosticResult
            {
                Id = "OA002",
                Message = "Async shoudn't be nested 3 times",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 9, 9)
                    }
            };

            var expected2 = new DiagnosticResult
            {
                Id = "OA002",
                Message = "Async shoudn't be nested 3 times",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 14, 9)
                    }
            };

            VerifyCSharpDiagnostic(test, expected1, expected2);
        }


        [TestMethod]
        public void TestAsyncDepthMultipleClasses()
        {
            const string test = @"
using System.Threading;
using System.Threading.Tasks;

namespace OverAsynchrony
{
    class Service
    {
        public async void Test0()
        {
            var service2 = new Service2();
            await service2.Test1();
        }
    }

    class Service2
    {
        public async void Test1()
        {
            await Test2();
        }

        public async void Test2()
        {
            await Test3();
        }
    }
    
}";

            var expected = new DiagnosticResult
            {
                Id = "OA002",
                Message = "Async shoudn't be nested 3 times",
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