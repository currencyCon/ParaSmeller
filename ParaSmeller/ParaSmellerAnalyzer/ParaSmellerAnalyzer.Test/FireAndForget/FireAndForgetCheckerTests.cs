using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParaSmeller.Test.Verifiers;
using ParaSmellerCore.Reporters;
using CodeFixVerifier = ParaSmeller.Test.Verifiers.CodeFixVerifier;

namespace ParaSmeller.Test.FireAndForget
{
    [TestClass]
    public class FireAndForgetCheckerTests: CodeFixVerifier
    {
        [TestMethod]
        public void TestFindsNotAwaitedTask()
        {
            const string test = @"
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Test
{
    public class TestCode
    {
        public static void X()
        {
            Thread.Sleep(5);
            Console.WriteLine(""Huhu"");
        }

        public static void Main()
        {
            var z = 3;
            Task.Run(() => X());
            Console.WriteLine(""Lol"");
        }
    }
}
";
            var expected = new DiagnosticResult
            {
                Id = FireAndForgetReporter.FireAndForgetCallId,
                Message = "The result of this Computation is potentially never awaited",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] {
                    new DiagnosticResultLocation("Test0.cs", 19, 13)
                }
            };
            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void TestReportsNotAwaitedAssignment()
        {
            const string test = @"
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Test
{
    public class TestCode
    {
        public static void X()
        {
            Thread.Sleep(5);
            Console.WriteLine(""Huhu"");
        }

        public static void Main()
        {
            var z = 3;
            var x = Task.Run(() => X());
            Console.WriteLine(""Lol"");
        }
    }
}
";
            var expected = new DiagnosticResult
            {
                Id = FireAndForgetReporter.FireAndForgetCallId,
                Message = "The result of this Computation is potentially never awaited",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] {
                    new DiagnosticResultLocation("Test0.cs", 19, 21)
                }
            };
            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void TestDoesNotReportAwaitedTask()
        {
            const string test = @"
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Test
{
    public class TestCode
    {
        public static void X()
        {
            Thread.Sleep(5);
            Console.WriteLine(""Huhu"");
        }

        public static void Main()
        {
            var z = 3;
            var x = Task.Run(() => X());
            x.Wait();
            Console.WriteLine(""Lol"");
        }
    }
}
";
            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void TestDoesNotReportWaitingBetweenMethods()
        {
            const string test = @"
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Test
{
    public class TestCode
    {
        public static void X()
        {
            Thread.Sleep(5);
            Console.WriteLine(""Huhu"");
        }

        public static void Main()
        {
            var z = 3;
            var x = Task.Run(() => X());
            DoWait(x);
            Console.WriteLine(""Lol"");
        }

        private static void DoWait(Task task)
        {
            task.Wait();
        }
    }
}
";
            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void TestDoesNotReportWaitingBetweenClasses()
        {
            const string test = @"
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Test
{
    public class TestCode
    {
        public static void X()
        {
            Thread.Sleep(5);
            Console.WriteLine(""Huhu"");
        }

        public static void Main()
        {
            var z = 3;
            var x = Task.Run(() => X());
            var w = new Waiter();
            w.Await(x);
            Console.WriteLine(""Lol"");
        }

        private static void Wait(Task task)
        {
            task.Wait();
        }
    }

    public class Waiter
    {
        public void Await(Task task)
        {
            task.Wait();
        }
    }
}
";
            VerifyCSharpDiagnostic(test);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new FireAndForgetCheckerAnalyzer();
        }
    }
}
