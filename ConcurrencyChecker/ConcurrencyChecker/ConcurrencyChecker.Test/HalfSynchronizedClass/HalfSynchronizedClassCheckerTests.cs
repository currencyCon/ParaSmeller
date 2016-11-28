using ConcurrencyAnalyzer.Reporters;
using ConcurrencyChecker.CodeFixProviders;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace ConcurrencyChecker.Test.HalfSynchronizedClass
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
        public void TestDetectsNoFalsePositiveOnSynchronizedPropertyWithNonSynchronizedLocals()
        {
            const string test = @"
                namespace bla
{
    class Program
    {
        private int _z;

        public int z
        {
            get
            {
                var x = 3;
                lock (this)
                {
                    if (_z > 4)
                    {
                        x = _z;
                    }
                }
                return x;
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
}";
            VerifyCSharpDiagnostic(test);

        }

        [TestMethod]
        public void TestDetectsNoFalsePositiveOnSynchronizedPropertyIgnoresEmptyMethods()
        {
            const string test = @"
namespace bla
{
    class Program
    {
        private int _z;

        public int z
        {
            get
            {
                var x = 3;
                lock (this)
                {
                    if (_z > 4)
                    {
                        x = _z;
                    }
                }
                return x;
            }

            set
            {
                lock (this)
                {
                    _z = value;
                }
            }
        }

        public void DoNothing()
        {
            
        }
        public void m()
        {
            lock (this)
            {
                z = 2;
                DoNothing();
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
                Id = HalfSynchronizedReporter.UnsynchronizedPropertyId,
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
                Id = HalfSynchronizedReporter.UnsynchronizedPropertyId,
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
        public void TestReportsNoFalsePositivesOnConstantGetters()
        {
            const string test = @"
namespace Test
{
    class TestProgram
    {
        public int z
        {
            get { return 1; }
        }

        public void m()
        {
            lock (this)
            {
                var x = z + 1;
            }
        }
    }
}
            ";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void TestReportsNoFalsePositivesOnConstantGettersComplexCase()
        {
            const string test = @"
namespace Test
{
    class TestProgram
    {
        public int z
        {
            get
            {
                var x = 2;
                return x;
            }
        }

        public void m()
        {
            lock (this)
            {
                var x = z + 1;
            }
        }
    }
}
            ";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void TestDetectsUnsynchronizedPropertyWithLockObject()
        {
            const string test = @"
                namespace Test
                {
                    public class LockClass
                    {
        
                    }
                    public class TestProgram
                    {
                        public TestProgram(LockClass lockClass)
                        {
                            LockObject = lockClass;
                        }
                        private LockClass LockObject { get;}

                        public int z {get; set;}

                        public void M()
                        {
                            lock (LockObject)
                            {
                                z = 2;
                            }
                        }
                    }
                }
            ";
            var expected = new DiagnosticResult
            {
                Id = HalfSynchronizedReporter.UnsynchronizedPropertyId,
                Message = "The Property is used in a synchronized Member. Consider synchronizing it.",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 16, 25)
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
                Id = HalfSynchronizedReporter.UnsynchronizedPropertyId,
                Message = "The Property is used in a synchronized Member. Consider synchronizing it.",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 6, 25)
                        }
            },
                new DiagnosticResult
            {
                Id = HalfSynchronizedReporter.HalfSynchronizedChildDiagnosticId,
                Message = "The Property  is also used in another synchronized Method . Consider synchronizing also this one.",
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
            VerifyCSharpFix(test, fixTest, warningId: HalfSynchronizedReporter.HalfSynchronizedChildDiagnosticId);
        }

        [TestMethod]
        public void TestProvidesHalfSynchronizedWithDefaultLockObjectFix()
        {
            const string test =
@"
namespace Test
{
    public class LockObject
    {
        
    }
    public class TestProgram
    {
        private LockObject LockObject { get; }

        public TestProgram(LockObject lockObject)
        {
            LockObject = lockObject;
        }
        public int z { get; set; }

        public void m()
        {
            lock (LockObject)
            {
                z = 2;
            }
        }

        public void m2()
        {
            z = 3;
        }
    }
}
";
            const string fixTest =
@"
namespace Test
{
    public class LockObject
    {
        
    }
    public class TestProgram
    {
        private LockObject LockObject { get; }

        public TestProgram(LockObject lockObject)
        {
            LockObject = lockObject;
        }
        public int z { get; set; }

        public void m()
        {
            lock (LockObject)
            {
                z = 2;
            }
        }

        public void m2()
        {
            lock (LockObject)
            {
                z = 3;
            }
        }
    }
}
";
            VerifyCSharpFix(test, fixTest, warningId: HalfSynchronizedReporter.HalfSynchronizedChildDiagnosticId);
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
            VerifyCSharpFix(test, fixTest, warningId: HalfSynchronizedReporter.UnsynchronizedPropertyId);
        }


        [TestMethod]
        public void TestProvidesSimpleUnsynchronizedPropertyFixWithCorrectLockObject()
        {
            const string test =
@"
namespace Test
{
    public class LockClass
    {
        
    }
    public class TestProgram
    {
        public TestProgram(LockClass lockClass)
        {
            LockObject = lockClass;
        }
        private LockClass LockObject { get;}

        public int z {get; set;}

        public void M()
        {
            lock (LockObject)
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
    public class LockClass
    {
        
    }
    public class TestProgram
    {
        public TestProgram(LockClass lockClass)
        {
            LockObject = lockClass;
        }
        private LockClass LockObject { get;}

        private int _z;

        public int z
        {
            get
            {
                lock (LockObject)
                {
                    return _z;
                }
            }

            set
            {
                lock (LockObject)
                {
                    _z = value;
                }
            }
        }

        public void M()
        {
            lock (LockObject)
            {
                z = 2;
            }
        }
    }
}
";
            VerifyCSharpFix(test, fixTest, warningId: HalfSynchronizedReporter.UnsynchronizedPropertyId);
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