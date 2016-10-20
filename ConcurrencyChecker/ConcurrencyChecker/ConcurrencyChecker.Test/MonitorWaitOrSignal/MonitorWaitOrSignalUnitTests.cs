using ConcurrencyChecker.MonitorWaitOrSignal;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace ConcurrencyChecker.Test.MonitorWaitOrSignal
{
    [TestClass]
    public class MonitorWaitOrSignalAnalyzerUnitTests : CodeFixVerifier
    {
        [TestMethod]
        public void TestNoDiagnostics()
        {
            var test = @"
using System.Threading;

namespace MonitorWaitOrSignalSmell
{
    class BoundedBuffer<T>
    {
        private Queue<T> queue = new Queue<T>();
        private const int Limit = 1;
        public void Put(T x)
        {
            lock (this)
            {
                while (queue.Count == Limit)
                {
                    Monitor.Wait(this);
                }
                queue.Enqueue(x);
                Monitor.PulseAll(this); // signal non-free
             }
        }
    }
}";
            VerifyCSharpDiagnostic(test);
        }


        [TestMethod]
        public void TestSimpleIfDiagnostics()
        {
            var test = @"
using System.Threading;

namespace MonitorWaitOrSignalSmell
{
    class BoundedBuffer<T>
    {
        private Queue<T> queue = new Queue<T>();
        private const int Limit = 2;
        public void Put(T x)
        {
            lock (this)
            {
                if (queue.Count == Limit)
                {
                    Monitor.Wait(this);
                }
                queue.Enqueue(x);
                Monitor.PulseAll(this); // signal non-free
            }
        }
        public T Get()
        {
            lock (this)
            {
                if (queue.Count == 0)
                {
                    Monitor.Wait(this);
                }
                T x = queue.Dequeue();
                Monitor.PulseAll(this); // signal non-full
                return x;
            }
        }
    }
}";

            var expected1 = new DiagnosticResult
            {
                Id = "MWS001",
                Message = "if should be replaced with while",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 14, 17)
                    }
            };

            var expected2 = new DiagnosticResult
            {
                Id = "MWS001",
                Message = "if should be replaced with while",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 26, 17)
                    }
            };

            VerifyCSharpDiagnostic(test, expected1, expected2);
        }

        [TestMethod]
        public void TestDoWhileNoDiagnostics()
        {
            var test = @"
using System.Threading;

namespace MonitorWaitOrSignalSmell
{
    class BoundedBuffer<T>
    {
        private Queue<T> queue = new Queue<T>();
        private const int Limit = 2;
        public void Put(T x)
        {
            lock (this)
            {
                do 
                {
                    Monitor.Wait(this);
                } while (queue.Count == Limit);
                queue.Enqueue(x);
                Monitor.PulseAll(this); // signal non-free
            }
        }
        public T Get()
        {
            lock (this)
            {
                do 
                {
                    Monitor.Wait(this);
                } while (queue.Count == 0);
                T x = queue.Dequeue();
                Monitor.PulseAll(this); // signal non-full
                return x;
            }
        }
    }
}";
            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void TestSimplePulseDiagnostics()
        {
            var test = @"
using System.Threading;

namespace MonitorWaitOrSignalSmell
{
    class BoundedBuffer<T>
    {
        private Queue<T> queue = new Queue<T>();
        private const int Limit = 2;
        public void Put(T x)
        {
            lock (this)
            {
                while (queue.Count == Limit)
                {
                    Monitor.Wait(this);
                }
                queue.Enqueue(x);
                Monitor.Pulse(this); // signal non-free
            }
        }
        public T Get()
        {
            lock (this)
            {
                while (queue.Count == 0)
                {
                    Monitor.Wait(this);
                }
                T x = queue.Dequeue();
                Monitor.Pulse(this); // signal non-full
                return x;
            }
        }
    }
}";

            var expected1 = new DiagnosticResult
            {
                Id = "MWS002",
                Message = "Pulse should be replaced with PulseAll",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 19, 17)
                    }
            };

            var expected2 = new DiagnosticResult
            {
                Id = "MWS002",
                Message = "Pulse should be replaced with PulseAll",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 31, 17)
                    }
            };

            VerifyCSharpDiagnostic(test, expected1, expected2);
        }


        [TestMethod]
        public void TestPulseReplacement()
        {
            var test = @"
using System.Threading;

namespace MonitorWaitOrSignalSmell
{
    class BoundedBuffer<T>
    {
        private Queue<T> queue = new Queue<T>();
        private const int Limit = 2;
        public void Put(T x)
        {
            lock (this)
            {
                while (queue.Count == Limit)
                {
                    Monitor.Wait(this);
                }
                queue.Enqueue(x);
                Monitor.Pulse(this); // signal non-free
            }
        }
        public T Get()
        {
            lock (this)
            {
                while (queue.Count == 0)
                {
                    Monitor.Wait(this);
                }
                T x = queue.Dequeue();
                Monitor.Pulse(this); // signal non-full
                return x;
            }
        }
    }
}";

            var fixtest = @"
using System.Threading;

namespace MonitorWaitOrSignalSmell
{
    class BoundedBuffer<T>
    {
        private Queue<T> queue = new Queue<T>();
        private const int Limit = 2;
        public void Put(T x)
        {
            lock (this)
            {
                while (queue.Count == Limit)
                {
                    Monitor.Wait(this);
                }
                queue.Enqueue(x);
                Monitor.PulseAll(this); // signal non-free
            }
        }
        public T Get()
        {
            lock (this)
            {
                while (queue.Count == 0)
                {
                    Monitor.Wait(this);
                }
                T x = queue.Dequeue();
                Monitor.PulseAll(this); // signal non-full
                return x;
            }
        }
    }
}";
            VerifyCSharpFix(test, fixtest, allowNewCompilerDiagnostics: true);
        }


        [TestMethod]
        public void TestIfReplacement()
        {
            var test = @"
using System.Threading;

namespace MonitorWaitOrSignalSmell
{
    class BoundedBuffer<T>
    {
        private Queue<T> queue = new Queue<T>();
        private const int Limit = 2;
        public void Put(T x)
        {
            lock (this)
            {
                if (queue.Count == Limit)
                {
                    Monitor.Wait(this);
                }
                queue.Enqueue(x);
                Monitor.PulseAll(this); // signal non-free
            }
        }
        public T Get()
        {
            lock (this)
            {
                if (queue.Count == 0)
                {
                    Monitor.Wait(this);
                }
                T x = queue.Dequeue();
                Monitor.PulseAll(this); // signal non-full
                return x;
            }
        }
    }
}";

            var fixtest = @"
using System.Threading;

namespace MonitorWaitOrSignalSmell
{
    class BoundedBuffer<T>
    {
        private Queue<T> queue = new Queue<T>();
        private const int Limit = 2;
        public void Put(T x)
        {
            lock (this)
            {
                while (queue.Count == Limit)
                {
                    Monitor.Wait(this);
                }
                queue.Enqueue(x);
                Monitor.PulseAll(this); // signal non-free
            }
        }
        public T Get()
        {
            lock (this)
            {
                while (queue.Count == 0)
                {
                    Monitor.Wait(this);
                }
                T x = queue.Dequeue();
                Monitor.PulseAll(this); // signal non-full
                return x;
            }
        }
    }
}";
            VerifyCSharpFix(test, fixtest, allowNewCompilerDiagnostics: true);
        }

        [TestMethod]
        public void TestsReplacements()
        {
            var test = @"
using System.Threading;

namespace MonitorWaitOrSignalSmell
{
    class BoundedBuffer<T>
    {
        private Queue<T> queue = new Queue<T>();
        private const int Limit = 2;
        public void Put(T x)
        {
            lock (this)
            {
                if (queue.Count == Limit)
                {
                    Monitor.Wait(this);
                }
                queue.Enqueue(x);
                Monitor.Pulse(this); // signal non-free
            }
        }
        public T Get()
        {
            lock (this)
            {
                if (queue.Count == 0)
                {
                    Monitor.Wait(this);
                }
                T x = queue.Dequeue();
                Monitor.Pulse(this); // signal non-full
                return x;
            }
        }
    }
}";

            var fixtest = @"
using System.Threading;

namespace MonitorWaitOrSignalSmell
{
    class BoundedBuffer<T>
    {
        private Queue<T> queue = new Queue<T>();
        private const int Limit = 2;
        public void Put(T x)
        {
            lock (this)
            {
                while (queue.Count == Limit)
                {
                    Monitor.Wait(this);
                }
                queue.Enqueue(x);
                Monitor.PulseAll(this); // signal non-free
            }
        }
        public T Get()
        {
            lock (this)
            {
                while (queue.Count == 0)
                {
                    Monitor.Wait(this);
                }
                T x = queue.Dequeue();
                Monitor.PulseAll(this); // signal non-full
                return x;
            }
        }
    }
}";
            VerifyCSharpFix(test, fixtest, allowNewCompilerDiagnostics: true);
        }

        [TestMethod]
        public void TestFunctionReferencesDiagnostics()
        {
            var test = @"
using System.Threading;

namespace MonitorWaitOrSignalSmell
{
    class BoundedBuffer<T>
    {
        private Queue<T> queue = new Queue<T>();
        private const int Limit = 2;
        public void Put(T x)
        {
            lock (this)
            {
                if (queue.Count == Limit) 
                {
                    DoSomeWaiting()
                } 
                queue.Enqueue(x);
                Monitor.PulseAll(this); // signal non-free
            }
        }
        
        private void DoSomeWaiting() 
        {
            int i = 0;
            Monitor.Wait(this);    
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = "MWS001",
                Message = "if should be replaced with while",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 14, 17)
                    }
            };
            
            VerifyCSharpDiagnostic(test, expected);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new MonitorWaitOrSignalCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new MonitorWaitOrSignalAnalyzer();
        }
    }
}