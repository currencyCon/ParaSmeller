using ConcurrencyChecker.TentativelyResourceReference;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace ConcurrencyChecker.Test.TentativelyResourceReference
{
    [TestClass]
    public class TentativelyResourceReferenceTests : CodeFixVerifier
    {
        [TestMethod]
        public void MutexTest()
        {
            const string test = @"
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Test
{
    public class BancAcount
    {
        public void Test(int amount)
        {
            Mutex a = new Mutex();
            a.WaitOne(1);
            a.WaitOne(new TimeSpan());
        }
    }
}";
            var expected1 = new DiagnosticResult
            {
                Id = TentativelyResourceReferenceAnalyzer.DiagnosticId,
                Message = TentativelyResourceReferenceAnalyzer.MessageFormatPrimitiveSynchronization.ToString(),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 13, 13)
                        }
            };

            var expected2 = new DiagnosticResult
            {
                Id = TentativelyResourceReferenceAnalyzer.DiagnosticId,
                Message = TentativelyResourceReferenceAnalyzer.MessageFormatPrimitiveSynchronization.ToString(),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 14, 13)
                        }
            };

            VerifyCSharpDiagnostic(test, expected1, expected2);
        }

        [TestMethod]
        public void MonitorTryEnterTest()
        {
            const string test = @"
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Test
{
    public class Queue
    {
        public bool TryEnqueue(int waitTime)
        {
            // Request the lock.
            if (Monitor.TryEnter(this, waitTime))
            {
                try
                {
                    int test = 10;
                }
                finally
                {
                    Monitor.Exit(this);
                }
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}";
            var expected = new DiagnosticResult
            {
                Id = TentativelyResourceReferenceAnalyzer.DiagnosticId,
                Message = TentativelyResourceReferenceAnalyzer.MessageFormatPrimitiveSynchronization.ToString(),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 13, 17)
                        }
            };
            
            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void MonitorWaitTest()
        {
            const string test = @"
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Test
{
    public class Queue
    {
        int counter = 0;
        public void TryDequeue(int waitTime)
        {
            lock(this) 
            {
                while(counter > 0)
                {
                    if(Monitor.Wait(this, waitTime))
                    {
                        int test = 10;
                    }
                }
            }
        }
    }
}";
            var expected = new DiagnosticResult
            {
                Id = TentativelyResourceReferenceAnalyzer.DiagnosticId,
                Message = TentativelyResourceReferenceAnalyzer.MessageFormatPrimitiveSynchronization.ToString(),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 17, 24)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }



        [TestMethod]
        public void BarrierSignalAndWaitTest()
        {
            const string test = @"
using System;
using System.Threading;

namespace Test
{
    public class Race
    {
        public void Round()
        {
            Barrier barrier = new Barrier(10);  
            barrier.SignalAndWait(10);
        }
    }
}";
            var expected = new DiagnosticResult
            {
                Id = TentativelyResourceReferenceAnalyzer.DiagnosticId,
                Message = TentativelyResourceReferenceAnalyzer.MessageFormatPrimitiveSynchronization.ToString(),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 11, 17)
                        }
            };

            //VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void SpinLockTryEnterTest()
        {
            const string test = @"
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Test
{
    public class Race
    {
        SpinLock s = new SpinLock();
        bool test = false;

        public void Round()
        {
            s.TryEnter(ref test);
        }
    }
}";
            var expected = new DiagnosticResult
            {
                Id = TentativelyResourceReferenceAnalyzer.DiagnosticId,
                Message = TentativelyResourceReferenceAnalyzer.MessageFormatPrimitiveSynchronization.ToString(),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 11, 17)
                        }
            };

            //VerifyCSharpDiagnostic(test, expected);
        }


        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new TentativelyResourceReferenceAnalyzer();
        }
    }
}
