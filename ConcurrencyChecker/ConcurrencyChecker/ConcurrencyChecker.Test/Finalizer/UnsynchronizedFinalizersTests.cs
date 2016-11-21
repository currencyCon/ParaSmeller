using ConcurrencyAnalyzer.Reporters;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace ConcurrencyChecker.Test.Finalizer
{
    [TestClass]
    public class UnsynchronizedFinalizersTests: CodeFixVerifier
    {
        [TestMethod]
        public void DoesNotReportFalsePositives()
        {
            const string test = @"
namespace ConcurrencyChecker.Test.TestCodeTester
{
    public class SynchronizedFinalizer
    {
        public static int Counter;
        private static readonly object LockObject = new object();
        public SynchronizedFinalizer()
        {
            lock (LockObject)
            {
                Counter++;
            }
        }

        ~SynchronizedFinalizer()
        {
            lock (LockObject)
            {
                Counter--;
            }
        }
    }
}";
            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void ReportsUnsynchronizedFieldAccess()
        {
            const string test = @"
namespace ConcurrencyChecker.Test.TestCodeTester
{
    public class SynchronizedFinalizer
    {
        public static int Counter;
        public SynchronizedFinalizer()
        {
            Counter++;
        }

        ~SynchronizedFinalizer()
        {
            Counter--;
        }

    }
}";
            var expected = new [] {
            
                new DiagnosticResult{
                Id = FinalizerReporter.FinalizerSynchronizationDiagnosticId,
                Message = FinalizerReporter.MessageFormatFinalizerSynchronization.ToString(),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 6, 9)
                        }
                },
                new DiagnosticResult{
                Id = FinalizerReporter.FinalizerSynchronizationDiagnosticId,
                Message = FinalizerReporter.MessageFormatFinalizerSynchronization.ToString(),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 14, 13)
                        }
                }
                };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void ReportsUnsynchronizedFieldAccessNoFalsePositivesNoNStatic()
        {
            const string test = @"
namespace ConcurrencyChecker.Test.TestCodeTester
{
    public class SynchronizedFinalizer
    {
        public int Counter;
        public SynchronizedFinalizer()
        {
            Counter++;
        }

        ~SynchronizedFinalizer()
        {
            Counter--;
        }

    }
}";
            VerifyCSharpDiagnostic(test);
        }
        [TestMethod]
        public void DoesNotReportFalsePositivesOnProperties()
        {
            const string test = @"
namespace ConcurrencyChecker.Test.TestCodeTester
{
    public class SynchronizedFinalizer
    {
        private static int _counter;

        public static int Counter
        {
            get {
                lock (LockObject)
                {
                    return _counter;
                }
            } 
            set {
                lock (LockObject)
                {
                    _counter = value;
                }
            }
        }

        private static readonly object LockObject = new object();
        public SynchronizedFinalizer()
        {
            lock (LockObject)
            {
                Counter++;
            }
        }

        ~SynchronizedFinalizer()
        {
            lock (LockObject)
            {
                Counter--;
            }
        }
    }
}";
            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void ReportsNotSynchronizedProperty()
        {
            const string test = @"
namespace ConcurrencyChecker.Test.TestCodeTester
{
    public class SynchronizedFinalizer
    {
        public static int Counter{get; set;}
        private static readonly object LockObject = new object();
        public SynchronizedFinalizer()
        {
            lock (LockObject)
            {
                Counter++;
            }
        }

        ~SynchronizedFinalizer()
        {
            lock (LockObject)
            {
                Counter--;
            }
        }
    }
}";
            var expected = new DiagnosticResult
            {
                Id = FinalizerReporter.FinalizerSynchronizationDiagnosticId,
                Message = FinalizerReporter.MessageFormatFinalizerSynchronization.ToString(),
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 6, 9)}
            };

            VerifyCSharpDiagnostic(test, expected);
        }
        [TestMethod]
        public void ReportsUnsynchronizedPropertyAccess()
        {
            const string test = @"
namespace ConcurrencyChecker.Test.TestCodeTester
{
    public class SynchronizedFinalizer
    {
        public static int Counter { get; set; }
        public SynchronizedFinalizer()
        {
            Counter++;
        }

        ~SynchronizedFinalizer()
        {
            Counter--;
        }
    }
}";
            var expected = new[] {

                new DiagnosticResult{
                Id = FinalizerReporter.FinalizerSynchronizationDiagnosticId,
                Message = FinalizerReporter.MessageFormatFinalizerSynchronization.ToString(),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 6, 9)
                        }
                },
                new DiagnosticResult{
                Id = FinalizerReporter.FinalizerSynchronizationDiagnosticId,
                Message = FinalizerReporter.MessageFormatFinalizerSynchronization.ToString(),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 14, 13)
                        }
                }
                };

            VerifyCSharpDiagnostic(test, expected);
        }


        [TestMethod]
        public void DoesNoticeWrongUsageOfLockObjectsOnFields()
        {
            const string test = @"
namespace ConcurrencyChecker.Test.TestCodeTester
{
    public class SynchronizedFinalizer
    {
        public static int Counter;
        public SynchronizedFinalizer()
        {
            lock (this)
            {
                Counter++;
            }
        }

        ~SynchronizedFinalizer()
        {
            lock (this)
            {
                Counter--;
            }
        }
    }
}";
            var expected = new[] {

                new DiagnosticResult{
                Id = FinalizerReporter.FinalizerSynchronizationDiagnosticId,
                Message = FinalizerReporter.MessageFormatFinalizerSynchronization.ToString(),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 11, 17)
                        }
                },
                new DiagnosticResult{
                Id = FinalizerReporter.FinalizerSynchronizationDiagnosticId,
                Message = FinalizerReporter.MessageFormatFinalizerSynchronization.ToString(),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 19, 17)
                        }
                }
                };
            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void DoesNoticeWrongUsageOfLockObjectsOnFieldsComplexCase()
        {
            const string test = @"
namespace ConcurrencyChecker.Test.TestCodeTester
{
    public class SynchronizedFinalizer
    {
        public static int Counter;
        private static readonly object LockObjectA = new object();
        private static readonly object LockObjectB = new object();

        public SynchronizedFinalizer()
        {
            lock (LockObjectA)
            {
                Counter++;
            }
        }

        ~SynchronizedFinalizer()
        {
            lock (LockObjectB)
            {
                Counter--;
            }
        }
    }
}";
            var expected = new[] {

                new DiagnosticResult{
                Id = FinalizerReporter.FinalizerSynchronizationDiagnosticId,
                Message = FinalizerReporter.MessageFormatFinalizerSynchronization.ToString(),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 14, 17)
                        }
                },
                new DiagnosticResult{
                Id = FinalizerReporter.FinalizerSynchronizationDiagnosticId,
                Message = FinalizerReporter.MessageFormatFinalizerSynchronization.ToString(),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 22, 17)
                        }
                }
                };
            VerifyCSharpDiagnostic(test, expected);
        }
        [TestMethod]
        public void DoesNoticeWrongUsageOfLockObjectsOnFieldsComplexCaseTwo()
        {
            const string test = @"
namespace ConcurrencyChecker.Test.TestCodeTester
{
    public class SynchronizedFinalizer
    {
        public static int Counter;
        private  readonly object LockObjectA = new object();

        public SynchronizedFinalizer()
        {
            lock (LockObjectA)
            {
                Counter++;
            }
        }

        ~SynchronizedFinalizer()
        {
            lock (LockObjectA)
            {
                Counter--;
            }
        }
    }
}";
            var expected = new[] {

                new DiagnosticResult{
                Id = FinalizerReporter.FinalizerSynchronizationDiagnosticId,
                Message = FinalizerReporter.MessageFormatFinalizerSynchronization.ToString(),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 13, 17)
                        }
                },
                new DiagnosticResult{
                Id = FinalizerReporter.FinalizerSynchronizationDiagnosticId,
                Message = FinalizerReporter.MessageFormatFinalizerSynchronization.ToString(),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 21, 17)
                        }
                }
                };
            VerifyCSharpDiagnostic(test, expected);
        }
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new FinalizerSynchronizationAnalyzer();
        }
    }
}
