using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParaSmeller.Test.Verifiers;
using ParaSmellerAnalyzer.CodeFixProviders;
using ParaSmellerCore.Reporters;
using CodeFixVerifier = ParaSmeller.Test.Verifiers.CodeFixVerifier;

namespace ParaSmeller.Test.MonitorWaitOrSignal
{
    [TestClass]
    public class MonitorWaitOrSignalAnalyzerUnitTests : CodeFixVerifier
    {
        [TestMethod]
        public void TestDoesntReportFalsePositives()
        {
            const string test = @"
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
        public void TestReportsSimpleWrongIfUsage()
        {
            const string test = @"
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

            var expected = new[] {
                new DiagnosticResult
                {
                    Id = MonitorOrWaitSignalReporter.MonitorIfConditionDiagnosticId,
                    Message = MonitorOrWaitSignalReporter.MessageFormatIf.ToString(),
                    Severity = DiagnosticSeverity.Warning,
                    Locations =
                        new[]
                        {
                            new DiagnosticResultLocation("Test0.cs", 14, 17)
                        }
                }, new DiagnosticResult
                {
                    Id = MonitorOrWaitSignalReporter.MonitorIfConditionDiagnosticId,
                    Message = MonitorOrWaitSignalReporter.MessageFormatIf.ToString(),
                    Severity = DiagnosticSeverity.Warning,
                    Locations =
                        new[]
                        {
                            new DiagnosticResultLocation("Test0.cs", 26, 17)
                        }
                }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void TestReportsNoFalsePositivesOnCorrectDoWhile()
        {
            const string test = @"
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
        public void TestReportsSimplePulseWrongUsage()
        {
            const string test = @"
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

            var expected = new [] {
                new DiagnosticResult
                {
                    Id = MonitorOrWaitSignalReporter.MonitorPulseDiagnosticId,
                    Message = MonitorOrWaitSignalReporter.MessageFormatPulse.ToString(),
                    Severity = DiagnosticSeverity.Warning,
                    Locations =
                        new[]
                        {
                            new DiagnosticResultLocation("Test0.cs", 19, 17)
                        }
                }, new DiagnosticResult
                {
                    Id = MonitorOrWaitSignalReporter.MonitorPulseDiagnosticId,
                    Message = MonitorOrWaitSignalReporter.MessageFormatPulse.ToString(),
                    Severity = DiagnosticSeverity.Warning,
                    Locations =
                        new[]
                        {
                            new DiagnosticResultLocation("Test0.cs", 31, 17)
                        }
                }
            };
            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void TestAppliesPulseReplacementFix()
        {
            const string test = @"
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

            const string fixtest = @"
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
        public void TestAppliesIfReplacementFix()
        {
            const string test = @"
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

            const string fixtest = @"
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
        public void TestsAppliesComplexReplacementsFixes()
        {
            const string test = @"
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

            const string fixtest = @"
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
        public void TestReportsFunctionReferencesWrongUsage()
        {
            const string test = @"
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
                Id = MonitorOrWaitSignalReporter.MonitorIfConditionDiagnosticId,
                Message = MonitorOrWaitSignalReporter.MessageFormatIf.ToString(),
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