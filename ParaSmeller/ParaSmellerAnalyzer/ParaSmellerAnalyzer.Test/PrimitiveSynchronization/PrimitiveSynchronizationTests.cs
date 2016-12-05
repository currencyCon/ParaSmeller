using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParaSmellerCore.Reporters;
using TestHelper;

namespace ParaSmeller.Test.PrimitiveSynchronization
{
    [TestClass]
    public class PrimitiveSynchronizationTests: CodeFixVerifier
    {
        [TestMethod]
        public void AnalyzerFindsInterlockedSimpleCase()
        {
            const string test = @"
using System.Threading;

namespace Test
{
    public class BancAcount
    {
        private int _balance;

        public void Withdraw(int amount)
        {
            var value = _balance;
            if (value >= _balance)
            {
                Interlocked.Add(ref _balance, -amount);
            }
        }
    }
}";
            var expected = new DiagnosticResult
            {
                Id = PrimitiveSynchronizationReporter.PrimitiveSynchronizationDiagnosticId,
                Message = PrimitiveSynchronizationReporter.MessageFormatPrimitiveSynchronization.ToString(),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 15, 17)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);

        }

        [TestMethod]
        public void AnalyzerFindsVolatileSimpleCase()
        {
            const string test = @"

namespace Test
{
    public class VolatileTest
    {
        public volatile int Vol;

        public void Test(int i)
        {
            Vol = i;
        }
    }
}";
            var expected = new DiagnosticResult
            {
                Id = PrimitiveSynchronizationReporter.PrimitiveSynchronizationDiagnosticId,
                Message = PrimitiveSynchronizationReporter.MessageFormatPrimitiveSynchronization.ToString(),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 7, 9)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);

        }

        [TestMethod]
        public void AnalyzerFindsVolatileReferences()
        {
            const string test = @"

namespace Test
{
    public class TestClass
    {
        public int MyInt;
    }
    public class VolatileTest
    {
        public volatile TestClass TestClass;

        public void Test(TestClass testClass)
        {
            TestClass = testClass;
        }
    }
}";
            var expected = new DiagnosticResult
            {
                Id = PrimitiveSynchronizationReporter.PrimitiveSynchronizationDiagnosticId,
                Message = PrimitiveSynchronizationReporter.MessageFormatPrimitiveSynchronization.ToString(),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 11, 9)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);

        }

        [TestMethod]
        public void AnalyzerFindsVolatileEnums()
        {
            const string test = @"

namespace Test
{
    public enum TestEnum
    {
        One,
        Tow,
        Three
    }
    public class VolatileTest
    {
        public volatile TestEnum TestEnum;

        public void Test(TestEnum testEnum)
        {
            TestEnum = testEnum;
        }
    }
}";
            var expected = new DiagnosticResult
            {
                Id = PrimitiveSynchronizationReporter.PrimitiveSynchronizationDiagnosticId,
                Message = PrimitiveSynchronizationReporter.MessageFormatPrimitiveSynchronization.ToString(),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 13, 9)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);

        }

        [TestMethod]
        public void AnalyzerFindsVolatileGenerics()
        {
            const string test = @"

namespace Test
{
    public class VolatileTest<TVol> where TVol : class
    {
        public volatile TVol VolatileElement;

        public void Test(TVol volatileElement)
        {
            VolatileElement = volatileElement;
        }
    }
}";
            var expected = new DiagnosticResult
            {
                Id = PrimitiveSynchronizationReporter.PrimitiveSynchronizationDiagnosticId,
                Message = PrimitiveSynchronizationReporter.MessageFormatPrimitiveSynchronization.ToString(),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 7, 9)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);

        }

        [TestMethod]
        public void AnalyzerFindsYieldUsage()
        {
            const string test = @"
using System.Threading;
using System.Threading.Tasks;

namespace Test
{
    public class YieldTest
    {
        private static void DoLongWork()
        {
            for (var i = 0; i < 1000; i++)
            {
                DoHardStuff();
                Thread.Yield();
            }
        }

        public static void Main()
        {
            var x = Task.Run(() => DoLongWork());
            x.Wait();
        }

        private static void DoHardStuff()
        {
            Thread.Sleep(100);
        }
    }
}";
            var expected = new DiagnosticResult
            {
                Id = PrimitiveSynchronizationReporter.PrimitiveSynchronizationDiagnosticId,
                Message = PrimitiveSynchronizationReporter.MessageFormatPrimitiveSynchronization.ToString(),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 14, 17)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);

        }

        [TestMethod]
        public void AnalyzerFindsSpinLockUsageComplexCase()
        {
            const string test = @"
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

class SpinLockDemo2
{
    const int N = 100000;
    static readonly Queue<Data> Queue = new Queue<Data>();
    private static readonly object Lock = new object();
    private static SpinLock _spinlock = new SpinLock();

    private class Data
    {
        public string Name { get; set; }
        public double Number { get; set; }
    }
    static void Main(string[] args)
    {

        // First use a standard lock for comparison purposes.
        UseLock();
        Queue.Clear();
        UseSpinLock();

        Console.WriteLine(""Press a key"");
        Console.ReadKey();

        }

        private static void UpdateWithSpinLock(Data d, int i)
        {
            var lockTaken = false;
            try
            {
                _spinlock.Enter(ref lockTaken);
                Queue.Enqueue(d);
            }
            finally
            {
                if (lockTaken) _spinlock.Exit(false);
            }
        }

        private static void UseSpinLock()
        {

            var sw = Stopwatch.StartNew();

            Parallel.Invoke(
                    () => {
                        for (var i = 0; i < N; i++)
                        {
                            UpdateWithSpinLock(new Data { Name = i.ToString(), Number = i }, i);
                        }
                    },
                    () => {
                        for (var i = 0; i < N; i++)
                        {
                            UpdateWithSpinLock(new Data { Name = i.ToString(), Number = i }, i);
                        }
                    }
                );
            sw.Stop();
            Console.WriteLine(""elapsed ms with spinlock: {0}"", sw.ElapsedMilliseconds);
        }

        private static void UpdateWithLock(Data d)
        {
            lock (Lock)
            {
                Queue.Enqueue(d);
            }
        }

        private static void UseLock()
        {
            var sw = Stopwatch.StartNew();

            Parallel.Invoke(
                    () => {
                        for (var i = 0; i < N; i++)
                        {
                            UpdateWithLock(new Data { Name = i.ToString(), Number = i });
                        }
                    },
                    () => {
                        for (var i = 0; i < N; i++)
                        {
                            UpdateWithLock(new Data { Name = i.ToString(), Number = i });
                        }
                    }
                );
            sw.Stop();
            Console.WriteLine(""elapsed ms with lock: {0}"", sw.ElapsedMilliseconds);
        }
    }";
            var expected = new [] {
            new DiagnosticResult {
                Id = PrimitiveSynchronizationReporter.PrimitiveSynchronizationDiagnosticId,
                Message = PrimitiveSynchronizationReporter.MessageFormatPrimitiveSynchronization.ToString(),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 13, 5)
                        }
            },
                        new DiagnosticResult {
                Id = PrimitiveSynchronizationReporter.PrimitiveSynchronizationDiagnosticId,
                Message = PrimitiveSynchronizationReporter.MessageFormatPrimitiveSynchronization.ToString(),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 38, 17)
                        }
            },
                                    new DiagnosticResult {
                Id = PrimitiveSynchronizationReporter.PrimitiveSynchronizationDiagnosticId,
                Message = PrimitiveSynchronizationReporter.MessageFormatPrimitiveSynchronization.ToString(),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 43, 32)
                        }
            }
            }
            ;

            VerifyCSharpDiagnostic(test, expected);

        }

        [TestMethod]
        public void AnalyzerFindsMemoryBarrier()
        {
            const string test = @"
using System;
using System.Threading;

namespace ParaSmeller.Test.TestCodeTester
{
    public class Foo
    {
        int _answer;
        bool _complete;

        public void A()
        {
            _answer = 123;
            Thread.MemoryBarrier();
            _complete = true;
            Thread.MemoryBarrier();    
        }

        public  void B()
        {
            Thread.MemoryBarrier();
            if (_complete)
            {
                Thread.MemoryBarrier();
                Console.WriteLine(_answer);
            }
        }
    }
}";
            var expected = new[] {
                new DiagnosticResult {
                    Id = PrimitiveSynchronizationReporter.PrimitiveSynchronizationDiagnosticId,
                    Message = PrimitiveSynchronizationReporter.MessageFormatPrimitiveSynchronization.ToString(),
                    Severity = DiagnosticSeverity.Warning,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 15, 13)
                            }
                },
                new DiagnosticResult {
                    Id = PrimitiveSynchronizationReporter.PrimitiveSynchronizationDiagnosticId,
                    Message = PrimitiveSynchronizationReporter.MessageFormatPrimitiveSynchronization.ToString(),
                    Severity = DiagnosticSeverity.Warning,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 17, 13)
                            }
                },
                new DiagnosticResult {
                    Id = PrimitiveSynchronizationReporter.PrimitiveSynchronizationDiagnosticId,
                    Message = PrimitiveSynchronizationReporter.MessageFormatPrimitiveSynchronization.ToString(),
                    Severity = DiagnosticSeverity.Warning,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 22, 13)
                            }
                },
                new DiagnosticResult {
                    Id = PrimitiveSynchronizationReporter.PrimitiveSynchronizationDiagnosticId,
                    Message = PrimitiveSynchronizationReporter.MessageFormatPrimitiveSynchronization.ToString(),
                    Severity = DiagnosticSeverity.Warning,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 25, 17)
                            }
                }
            }
            ;

            VerifyCSharpDiagnostic(test, expected);

        }
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new PrimitiveSynchronizationAnalyzer();
        }
    }
}
