using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParaSmeller.Test.Verifiers;
using ParaSmellerCore.Reporters;
using CodeFixVerifier = ParaSmeller.Test.Verifiers.CodeFixVerifier;

namespace ParaSmeller.Test.OverAsynchrony
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
        public void TestReportsPrivateAsync()
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
                Id = OverAsynchronyReporter.DiagnosticId,
                Message = OverAsynchronyReporter.MessageFormat.ToString(),
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
        public void TestReportsAsyncOverDepth()
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
                Id = OverAsynchronyReporter.DiagnosticIdNestedAsync,
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
        public void TestNofalsePositivesOnAsyncDepth()
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
        public void TestNofalsePositivesOnAsyncDepthComplexCase()
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
            await Test3();
        }

        public async void Test2()
        {
            await Task.Run(() => { Thread.Sleep(1000); });
        }

        public async void Test3()
        {
            await Task.Run(() => { Thread.Sleep(1000); });
        }
    }
}";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void TestReportsAsyncOverDepthMultipleOccurence()
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

            var expected = new [] {
                new DiagnosticResult
                {
                    Id = OverAsynchronyReporter.DiagnosticIdNestedAsync,
                    Message = "Async shoudn't be nested 3 times",
                    Severity = DiagnosticSeverity.Warning,
                    Locations =
                        new[]
                        {
                            new DiagnosticResultLocation("Test0.cs", 9, 9)
                        }
                }, new DiagnosticResult
                {
                    Id = OverAsynchronyReporter.DiagnosticIdNestedAsync,
                    Message = "Async shoudn't be nested 3 times",
                    Severity = DiagnosticSeverity.Warning,
                    Locations =
                        new[]
                        {
                            new DiagnosticResultLocation("Test0.cs", 14, 9)
                        }
                }
            };

            VerifyCSharpDiagnostic(test, expected);
        }


        [TestMethod]
        public void TestReportsAsyncOverDepthMultipleClasses()
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
            if(true) 
            {
                await Test2();
            }
        }

        public async void Test2()
        {
            await Test3();
        }
    }
    
}";
            var expected = new DiagnosticResult
            {
                Id = OverAsynchronyReporter.DiagnosticIdNestedAsync,
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
        public void TestReportsOnAsyncDepthRecursive()
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
            await Test1();
        }
    }
    
}";

            var expected = new [] {
                new DiagnosticResult
                {
                Id = OverAsynchronyReporter.DiagnosticIdNestedAsync,
                Message = "Async shoudn't be nested 3 times",
                    Severity = DiagnosticSeverity.Warning,
                    Locations =
                        new[]
                        {
                            new DiagnosticResultLocation("Test0.cs", 9, 9)
                        }
                },
                new DiagnosticResult
                {
                Id = OverAsynchronyReporter.DiagnosticIdNestedAsync,
                Message = "Async shoudn't be nested 3 times",
                    Severity = DiagnosticSeverity.Warning,
                    Locations =
                        new[]
                        {
                            new DiagnosticResultLocation("Test0.cs", 18, 9)
                        }
                }, 
                new DiagnosticResult
                {
                Id = OverAsynchronyReporter.DiagnosticIdNestedAsync,
                Message = "Async shoudn't be nested 3 times",
                    Severity = DiagnosticSeverity.Warning,
                    Locations =
                        new[]
                        {
                            new DiagnosticResultLocation("Test0.cs", 24, 9)
                        }
                }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new OverAsynchronyAnalyzer();
        }
    }
}