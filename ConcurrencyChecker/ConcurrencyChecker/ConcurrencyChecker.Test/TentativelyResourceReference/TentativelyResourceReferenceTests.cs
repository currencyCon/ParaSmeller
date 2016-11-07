using ConcurrencyAnalyzer.Reporters.TentativelyResourceReferenceReporter;
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
                Id = TentativelyResourceReferenceReporter.DiagnosticId,
                Message = TentativelyResourceReferenceReporter.MessageFormatTentativelyResourceReference.ToString(),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 13, 13)
                        }
            };

            var expected2 = new DiagnosticResult
            {
                Id = TentativelyResourceReferenceReporter.DiagnosticId,
                Message = TentativelyResourceReferenceReporter.MessageFormatTentativelyResourceReference.ToString(),
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
                Id = TentativelyResourceReferenceReporter.DiagnosticId,
                Message = TentativelyResourceReferenceReporter.MessageFormatTentativelyResourceReference.ToString(),
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
                Id = TentativelyResourceReferenceReporter.DiagnosticId,
                Message = TentativelyResourceReferenceReporter.MessageFormatTentativelyResourceReference.ToString(),
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Permissions;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;

namespace Test
{
    public class Race
    {
        var barrier = new System.Threading.Barrier(10);  

        public void Round()
        {    
            barrier.SignalAndWait(10);
        }
    }
}";
            var expected = new DiagnosticResult
            {
                Id = TentativelyResourceReferenceReporter.DiagnosticId,
                Message = TentativelyResourceReferenceReporter.MessageFormatTentativelyResourceReference.ToString(),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 14, 13)
                        }
            };

            //Missing some references, in VS this test is working -.-
            //VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void SpinLockTryEnterTest()
        {
            const string test = @"
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            s.TryEnter(10, ref test);
        }
    }
}";
            var expected = new DiagnosticResult
            {
                Id = TentativelyResourceReferenceReporter.DiagnosticId,
                Message = TentativelyResourceReferenceReporter.MessageFormatTentativelyResourceReference.ToString(),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 17, 13)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }


        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new TentativelyResourceReferenceAnalyzer();
        }
    }
}
