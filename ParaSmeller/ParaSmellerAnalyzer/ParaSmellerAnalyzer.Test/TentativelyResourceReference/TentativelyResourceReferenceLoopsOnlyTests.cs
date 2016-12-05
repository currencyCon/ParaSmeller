using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParaSmeller.Test.Verifiers;
using ParaSmellerCore.Reporters;
using CodeFixVerifier = ParaSmeller.Test.Verifiers.CodeFixVerifier;

namespace ParaSmeller.Test.TentativelyResourceReference
{
    [TestClass]
    public class TentativelyResourceReferenceLoopsOnlyTests : CodeFixVerifier
    {
        [TestMethod]
        public void TestReportsMutex()
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
            while(1==1)
            {
                a.WaitOne(1);
                a.WaitOne(new TimeSpan());
            }
        }
    }
}";
            var expected = new [] {
                new DiagnosticResult
                {
                    Id = TentativelyResourceReferenceReporter.DiagnosticId,
                    Message = TentativelyResourceReferenceReporter.MessageFormatTentativelyResourceReference.ToString(),
                    Severity = DiagnosticSeverity.Warning,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 15, 17)
                            }
                },
                new DiagnosticResult
                {
                    Id = TentativelyResourceReferenceReporter.DiagnosticId,
                    Message = TentativelyResourceReferenceReporter.MessageFormatTentativelyResourceReference.ToString(),
                    Severity = DiagnosticSeverity.Warning,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 16, 17)
                            }
                }
            };
            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void TestReportsMonitorTryEnter()
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
            while (1==1) 
            {
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
    }
}";
            var expected = new DiagnosticResult
            {
                Id = TentativelyResourceReferenceReporter.DiagnosticId,
                Message = TentativelyResourceReferenceReporter.MessageFormatTentativelyResourceReference.ToString(),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 15, 21)
                        }
            };
            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void TestReportsMonitorWait()
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
        public void TestReportsSpinLockTryEnter()
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
            while(s.TryEnter(10, ref test)) 
            {
                
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
                            new DiagnosticResultLocation("Test0.cs", 17, 19)
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
