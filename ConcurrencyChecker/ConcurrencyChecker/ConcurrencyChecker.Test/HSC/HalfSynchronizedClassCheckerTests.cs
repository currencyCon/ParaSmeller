using ConcurrencyChecker.HalfSynchronizedChecker;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace ConcurrencyChecker.Test.HSC
{
    [TestClass]
    public class UnitTest : CodeFixVerifier
    {

        [TestMethod]
        public void TestDetectsNoFalsePositiveOnSynchronizedProperty()
        {
            const string test = @"
                namespace bla {
                    class Program
                    {
                        private int _z;

                        public int z
                        {
                            get
                            {
                                lock (this)
                                {
                                    return _z;
                                }
                            }

                            set
                            {
                                lock (this)
                                {
                                    _z = value;
                                }
                            }
                        }

                        public void m()
                        {
                            lock(this)
                            {
                                z = 2;
                            }
                        }
                    }
                }";
            VerifyCSharpDiagnostic(test);

        }

        [TestMethod]
        public void TestDetectsUnsynchronizedProperty()
        {
            const string test = @"
                namespace bla {
                    class Program
                    {
                        private int _z;

                        public int z
                        {
                            get
                            {
                                return _z;
                            }

                            set
                            {
                                lock (this)
                                {
                                    _z = value;
                                }
                            }
                        }

                        public void m()
                        {
                            lock(this)
                            {
                                z = 2;
                            }
                        }
                    }
                }";
            var expected = new DiagnosticResult
            {
                Id = HalfSynchronizedCheckerAnalyzer.UnsynchronizedPropertyId,
                Message = "The Property is used in a synchronized Member. Consider synchronizing it.",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] {
                    new DiagnosticResultLocation("Test0.cs", 7, 25)
                }
            };

            VerifyCSharpDiagnostic(test, expected);
        }
        [TestMethod]
        public void TestDetectsUnsynchronizedPropertySimpleCase()
        {
            const string test = @"
                namespace Test
                {
                    class TestProgram
                    {
                        public int z { get; set; }

                        public void m()
                        {
                            lock(this)
                            {
                                z = 2;
                            }
                        }
                    }
                }
            ";
            var expected = new DiagnosticResult
            {
                Id = HalfSynchronizedCheckerAnalyzer.UnsynchronizedPropertyId,
                Message = "The Property is used in a synchronized Member. Consider synchronizing it.",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 6, 25)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void TestDetectsHalfSynchronizedProperty()
        {
            const string test = @"
                namespace Test
                {
                    class TestProgram
                    {
                        public int z { get; set; }

                        public void m2() {
                            z = 3;
                        }
                        public void m()
                        {
                            lock(this)
                            {
                                z = 2;
                            }
                        }
                    }
                }
            ";
            var expected = new [] {
                new DiagnosticResult
            {
                Id = HalfSynchronizedCheckerAnalyzer.UnsynchronizedPropertyId,
                Message = "The Property is used in a synchronized Member. Consider synchronizing it.",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 6, 25)
                        }
            },
                new DiagnosticResult
            {
                Id = HalfSynchronizedCheckerAnalyzer.HalfSynchronizedChildDiagnosticId,
                Message = "The Property z is also used in another synchronized Method . Consider synchronizing also this one.",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 8, 25)
                        }
            }
            } ;

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void TestProvidesSimpleHalfSynchronizedFix()
        {
            const string test = 
@"
namespace Test
{
    class TestProgram
    {
        public int z { get; set; }

        public void m()
        {
            lock (this)
            {
                z = 2;
            }
        }

        public void m2() {
            z = 3;
        }
    }
}
";
            const string fixTest = 
@"
namespace Test
{
    class TestProgram
    {
        public int z { get; set; }

        public void m()
        {
            lock (this)
            {
                z = 2;
            }
        }

        public void m2()
        {
            lock (this)
            {
                z = 3;
            }
        }
    }
}
";
            VerifyCSharpFix(test, fixTest, warningId:HalfSynchronizedCheckerAnalyzer.HalfSynchronizedChildDiagnosticId);
        }


        [TestMethod]
        public void TestProvidesSimpleUnsynchronizedPropertyFix()
        {
            const string test =
@"
namespace Test
{
    class TestProgram
    {
        public int z { get; set; }

        public void m()
        {
            lock (this)
            {
                z = 2;
            }
        }
    }
}
";
            const string fixTest =
@"
namespace Test
{
    class TestProgram
    {
        private int _z;

        public int z
        {
            get
            {
                lock (this)
                {
                    return _z;
                }
            }

            set
            {
                lock (this)
                {
                    _z = value;
                }
            }
        }

        public void m()
        {
            lock (this)
            {
                z = 2;
            }
        }
    }
}
";
            VerifyCSharpFix(test, fixTest, warningId: HalfSynchronizedCheckerAnalyzer.UnsynchronizedPropertyId);
        }
        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new HalfSynchronizedCheckerCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new HalfSynchronizedCheckerAnalyzer();
        }
    }
}