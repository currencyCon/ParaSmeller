using ConcurrencyChecker.NestedSynchronizedMethodClassChecker;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace ConcurrencyChecker.Test.NSMC
{
    [TestClass]
    public class UnitTest : CodeFixVerifier
    {
        
        [TestMethod]
        public void AnalyzerFindsSimpleCase()
        {
            var test = @"
    class BankAccount
    {
        private int balance;
        public void Deposit(int amount)
        {
            lock (this) { balance += amount; }
        }
        public void Transfer(BankAccount target, int amount)
        {
            lock (this)
            {
                balance -= amount;
                target.Deposit(amount); // lock (target)
            }
        }
    }";
            var expected = new DiagnosticResult
            {
                Id = NestedSynchronizedMethodCalssAnalyzer.NestedLockingDiagnosticId,
                Message = "Possible Deadlock with double Locking",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 14, 17)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);

  }


        [TestMethod]
        public void AnalyzerFindMultiParameters()
        {
            const string test = @"
    class BankAccount
    {
        private int balance;
        public void Deposit(int amount)
        {
            lock (this) { balance += amount; }
        }
        public void Transfer(BankAccount target1, BankAccount target2, int amount)
        {
            lock (this)
            {
                balance -= amount;
                target1.Deposit(amount); 
                target2.Deposit(amount); 
            }
        }
    }";
            var expected1 = new DiagnosticResult
            {
                Id = NestedSynchronizedMethodCalssAnalyzer.NestedLockingDiagnosticId,
                Message = "Possible Deadlock with double Locking",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 14, 17)
                        }
            };

            var expected2 = new DiagnosticResult
            {
                Id = NestedSynchronizedMethodCalssAnalyzer.NestedLockingDiagnosticId,
                Message = "Possible Deadlock with double Locking",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 15, 17)
                        }
            };

            VerifyCSharpDiagnostic(test, expected1, expected2);

        }

        [TestMethod]
        public void AnalyzerNoSmellDifferentLock()
        {
            var test = @"
    class BankAccount
    {
        private int balance;
        private object lockObj = new object();
        public void Deposit(int amount)
        {
            lock (lockObj) { balance += amount; }
        }
        public void Transfer(BankAccount target, int amount)
        {
            lock (this)
            {
                balance -= amount;
                target.Deposit(amount); 
            }
        }
    }";
            
            VerifyCSharpDiagnostic(test);

        }

        [TestMethod]
        public void AnalyzerNoSmell()
        {
            var test = @"
    class BankAccount
    {
        private int balance;
        private object lockObj = new object();
        public void Deposit(int amount)
        {
            lock (lockObj) { balance += amount; }
        }
        public void Transfer(BankAccount target, int amount)
        {
            lock (this)
            {
                balance -= amount;
                target.Deposit(amount); 
            }
        }
    }";

            VerifyCSharpDiagnostic(test);

        }



        [TestMethod]
        public void AnalyzerMultipleLocks()
        {
            var test = @"
    class BankAccount
    {
        private int balance;
        private object lockObj = new object();
        public void Deposit(int amount)
        {
            lock (lockObj) { balance += amount; }
        }
        public void Transfer(BankAccount target, int amount)
        {
            lock (this)
            {
                balance -= amount;
                target.Deposit(amount); 
            }

            lock(lockObj) 
            {
                balance -= amount;
                target.Deposit(amount); 
            }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = NestedSynchronizedMethodCalssAnalyzer.NestedLockingDiagnosticId,
                Message = "Possible Deadlock with double Locking",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 21, 17)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);

        }


        [TestMethod]
        public void AnalyzerFindsInterface()
        {
            var test = @"
    internal interface IBankAccount
    {
        void Transfer(IBankAccount target, int amount);
        void Deposit(int amount);
    }

    class BankAccount : IBankAccount
    {
        private int balance;
        public void Deposit(int amount)
        {
            lock (this) { balance += amount; }
        }
        public void Transfer(IBankAccount target, int amount)
        {
            lock (this)
            {
                balance -= amount;
                target.Deposit(amount); // lock (target)
            }
        }
    }";
            var expected = new DiagnosticResult
            {
                Id = NestedSynchronizedMethodCalssAnalyzer.NestedLockingDiagnosticId,
                Message = "Possible Deadlock with double Locking",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 20, 17)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);

        }


        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new NestedSynchronizedMethodClassCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new NestedSynchronizedMethodCalssAnalyzer();
        }
    }
}