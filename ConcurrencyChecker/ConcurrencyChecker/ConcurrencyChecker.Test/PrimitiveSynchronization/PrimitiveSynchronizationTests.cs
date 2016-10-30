using ConcurrencyChecker.PrimitiveSynchronizationChecker;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace ConcurrencyChecker.Test.PrimitiveSynchronization
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
                Id = PrimitiveSynchronizationAnalyzer.PrimitiveSynchronizationDiagnosticId,
                Message = PrimitiveSynchronizationAnalyzer.MessageFormatPrimitiveSynchronization.ToString(),
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
                Id = PrimitiveSynchronizationAnalyzer.PrimitiveSynchronizationDiagnosticId,
                Message = PrimitiveSynchronizationAnalyzer.MessageFormatPrimitiveSynchronization.ToString(),
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
                Id = PrimitiveSynchronizationAnalyzer.PrimitiveSynchronizationDiagnosticId,
                Message = PrimitiveSynchronizationAnalyzer.MessageFormatPrimitiveSynchronization.ToString(),
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
                Id = PrimitiveSynchronizationAnalyzer.PrimitiveSynchronizationDiagnosticId,
                Message = PrimitiveSynchronizationAnalyzer.MessageFormatPrimitiveSynchronization.ToString(),
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
                Id = PrimitiveSynchronizationAnalyzer.PrimitiveSynchronizationDiagnosticId,
                Message = PrimitiveSynchronizationAnalyzer.MessageFormatPrimitiveSynchronization.ToString(),
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
                Id = PrimitiveSynchronizationAnalyzer.PrimitiveSynchronizationDiagnosticId,
                Message = PrimitiveSynchronizationAnalyzer.MessageFormatPrimitiveSynchronization.ToString(),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 14, 17)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);

        }
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new PrimitiveSynchronizationAnalyzer();
        }
    }
}
