using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParaSmeller.Test.Verifiers;
using ParaSmellerCore.Reporters;
using CodeFixVerifier = ParaSmeller.Test.Verifiers.CodeFixVerifier;

namespace ParaSmeller.Test.NestedSynchronizedMethodClass
{
    [TestClass]
    public class NestedSynchronizedMethodClassTest : CodeFixVerifier
    {
        
        [TestMethod]
        public void TestReportsSimpleCase()
        {
            const string test = @"
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
                Id = NestedSynchronizedMethodClassReporter.NestedLockingDiagnosticId,
                Message = NestedSynchronizedMethodClassReporter.MessageFormat.ToString(),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 14, 13)
                        }
            };
            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void TestReportsOnMultipleParameters()
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
            var expected = new [] {
                new DiagnosticResult
                {
                    Id = NestedSynchronizedMethodClassReporter.NestedLockingDiagnosticId,
                    Message = NestedSynchronizedMethodClassReporter.MessageFormat.ToString(),
                    Severity = DiagnosticSeverity.Warning,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 14, 13)
                            }
                },
                new DiagnosticResult
                {
                    Id = NestedSynchronizedMethodClassReporter.NestedLockingDiagnosticId,
                    Message = NestedSynchronizedMethodClassReporter.MessageFormat.ToString(),
                    Severity = DiagnosticSeverity.Warning,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 15, 13)
                            }
                }
            };
            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void TestDoesntReportOnDifferentLocks()
        {
            const string test = @"
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
        public void TestDoesntReportOnCorrectLockUsage()
        {
            const string test = @"
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
        public void TestReportsOnMultipleLockUsage()
        {
            const string test = @"
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
                Id = NestedSynchronizedMethodClassReporter.NestedLockingDiagnosticId,
                Message = NestedSynchronizedMethodClassReporter.MessageFormat.ToString(),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 21, 13)
                        }
            };
            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void TestCorrectlyFollowsInterfaces()
        {
            const string test = @"
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
                Id = NestedSynchronizedMethodClassReporter.NestedLockingDiagnosticId,
                Message = NestedSynchronizedMethodClassReporter.MessageFormat.ToString(),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 20, 13)
                        }
            };
            VerifyCSharpDiagnostic(test, expected);
        }


        [TestMethod]
        public void TestReportsOnMultipleLocksAquire()
        {
            const string test = @"
internal interface IBankAccount
{
    void Transfer(IBankAccount target, int amount);
    void Deposit(int amount);
}

class BankAccount : IBankAccount
{
    private object LockA = new object();
    private object LockB = new object();
    private object LockC = new object();

    public void TransferB()
    {
        lock (LockB)
        {
            lock(LockA) 
            {
                int i = 10;
            }
        }
    }
    
    public void TransferA()
    {
        lock (LockA)
        {
            lock(LockB) 
            {
                int i = 10;
                lock(LockC) 
                {
                    int j = 20;
                }
            }
        }
    }
}";
            var expected = new [] {
                new DiagnosticResult
                {
                    Id = NestedSynchronizedMethodClassReporter.NestedLockingDiagnosticId2,
                    Message = NestedSynchronizedMethodClassReporter.MessageFormat.ToString(),
                    Severity = DiagnosticSeverity.Warning,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 14, 17)
                            }
                },
                new DiagnosticResult
                {
                    Id = NestedSynchronizedMethodClassReporter.NestedLockingDiagnosticId2,
                    Message = NestedSynchronizedMethodClassReporter.MessageFormat.ToString(),
                    Severity = DiagnosticSeverity.Warning,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 25, 17)
                            }
                }
            };
            VerifyCSharpDiagnostic(test, expected);

        }

        [TestMethod]
        public void TestReportsOnMultipleLocksAquirementComplexCase()
        {
            const string test = @"
internal interface IBankAccount
{
    void Transfer(IBankAccount target, int amount);
    void Deposit(int amount);
}

class BankAccount : IBankAccount
{
    private object LockA = new object();
    private object LockB = new object();
    private object LockC = new object();

    public void TransferB()
    {
        lock (LockB)
        {
            lock(LockA) 
            {
                int i = 10;
                lock(LockC) 
                {
                    int j = 20;
                }
            }
        }
    }
    
    public void TransferA()
    {
        lock (LockB)
        {
            lock(LockC) 
            {
                int i = 10;
                lock(LockA) 
                {
                    int j = 20;
                }
            }
        }
    }
}";
            var expected = new [] {
                new DiagnosticResult
                {
                    Id = NestedSynchronizedMethodClassReporter.NestedLockingDiagnosticId2,
                    Message = NestedSynchronizedMethodClassReporter.MessageFormat.ToString(),
                    Severity = DiagnosticSeverity.Warning,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 14, 17)
                            }
                },
                new DiagnosticResult
                {
                    Id = NestedSynchronizedMethodClassReporter.NestedLockingDiagnosticId2,
                    Message = NestedSynchronizedMethodClassReporter.MessageFormat.ToString(),
                    Severity = DiagnosticSeverity.Warning,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 29, 17)
                            }
                }
            };
            VerifyCSharpDiagnostic(test, expected);
        }


        [TestMethod]
        public void TestNoFalsePositivesOnCorrectMultipleLockUsageComplexCase()
        {
            const string test = @"
class BankAccount
{
    private object LockA = new object();
    private object LockB = new object();
    private object LockC = new object();

    public void TransferB()
    {
        lock (LockA)
        {
            lock(LockB) 
            {
                int i = 10;
            }
        }
    }
    
    public void TransferA()
    {
        lock (LockA)
        {
            lock(LockB) 
            {
                int i = 10;
                lock(LockC) 
                {
                    int j = 20;
                }
            }
        }
    }
}";
            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void TestNoFalsePositivesOnCorrectMultipleLockUsage()
        {
            const string test = @"
class BankAccount
{
    private object LockA = new object();
    private object LockB = new object();
    private object LockC = new object();

    public void TransferB()
    {
        lock (LockA)
        {
            lock(LockB) 
            {
                int i = 10;
            }
        }
    }
    
    public void TransferA()
    {
        lock (LockA)
        {
            lock(LockC) 
            {
                int i = 10;
            }
        }
    }
}";
            VerifyCSharpDiagnostic(test);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new NestedSynchronizedMethodClassAnalyzer();
        }
    }
}