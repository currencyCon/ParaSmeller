using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParaSmellerCore.Representation;

namespace ParaSmeller.Test.Core
{
    [TestClass]
    public class CoreUnitTest
    {

        [TestMethod]
        public void TestSimpleSubClasses()
        {
            const string test = @"
namespace ParaSmeller.Test.Core
{
    public class Base
    {
        
    }

    public class Sub : Base
    {
        
    }

    public class SubSub : Sub
    {
        
    }
}";
            var solution = TestSolutionBuilder.CreateSolution(test);
            Assert.AreEqual(3, solution.Classes.Count);
            Assert.AreEqual(3, solution.ClassMap.Count);

            Assert.AreEqual(0, solution.ClassMap["ParaSmeller.Test.Core.Base"].ToArray()[0].ClassMap.Count);
            Assert.AreEqual(1, solution.ClassMap["ParaSmeller.Test.Core.Sub"].ToArray()[0].ClassMap.Count);
            Assert.AreEqual(2, solution.ClassMap["ParaSmeller.Test.Core.SubSub"].ToArray()[0].ClassMap.Count);

            Assert.AreEqual(2, solution.Classes.ToArray()[0].ClassMap.Count);
            Assert.AreEqual(1, solution.Classes.ToArray()[1].ClassMap.Count);
            Assert.AreEqual(0, solution.InterfaceMap.Count);
        }


        [TestMethod]
        public void TestSimpleInterfaces()
        {
            const string test = @"
namespace ParaSmeller.Test.Core
{
    public interface IBase 
    {

    }

    public class Base : IBase
    {
        
    }

    public class Sub : Base, ISub
    {
        
    }

    public interface ISub
    {
        
    }
}";
            var solution = TestSolutionBuilder.CreateSolution(test);
            Assert.AreEqual(2, solution.Classes.Count);
            Assert.AreEqual(2, solution.ClassMap.Count);
            Assert.AreEqual(2, solution.InterfaceMap.Count);

            Assert.AreEqual(1, solution.ClassMap["ParaSmeller.Test.Core.Sub"].ToArray()[0].ClassMap.Count);
            Assert.AreEqual(2, solution.InterfaceMap["ParaSmeller.Test.Core.IBase"].ImplementingClasses.Count);
            Assert.AreEqual(1, solution.InterfaceMap["ParaSmeller.Test.Core.ISub"].ImplementingClasses.Count);
        }


        [TestMethod]
        public void TestPartialClasses()
        {
            const string test = @"
namespace ParaSmeller.Test.Core
{
    public partial class Base
    {
        public void Method1() {}
    }

    public partial class Base
    {
        public void Method2() {}
    }

}";
            var solution = TestSolutionBuilder.CreateSolution(test);
            Assert.AreEqual(2, solution.Classes.Count);
            Assert.AreEqual(1, solution.ClassMap.Count);
            Assert.AreEqual(2, solution.ClassMap["ParaSmeller.Test.Core.Base"].Count);
        }

        [TestMethod]
        public void TestMethodInvocations()
        {
            const string test = @"
namespace ParaSmeller.Test.Core
{
    public class A
    {
        public void Method1() 
        {
            B b = new B();
            b.Method2();
        }
    }

    public class B
    {
        public void Method2() {}
    }

}";
            var solution = TestSolutionBuilder.CreateSolution(test);
            Assert.AreEqual(2, solution.Classes.Count);
            Assert.AreEqual(2, solution.ClassMap.Count);

            var method1 = (solution.ClassMap["ParaSmeller.Test.Core.A"].ToArray()[0].Methods.ToArray()[0]);
            Assert.IsNotNull(method1);
            Assert.AreEqual(1, method1.Blocks.Count);
            Assert.AreEqual(1, method1.Blocks.ToArray()[0].InvocationExpressions.Count);
            var invocationMethodB = method1.Blocks.ToArray()[0].InvocationExpressions.ToArray()[0];
            Assert.AreEqual("ParaSmeller.Test.Core.B", invocationMethodB.CalledClassOriginal);
            Assert.AreEqual("ParaSmeller.Test.Core.B.Method2()", invocationMethodB.OriginalDefinition);
            Assert.AreEqual(1, invocationMethodB.InvokedImplementations.Count);
        }


        [TestMethod]
        public void TestLockBLock()
        {
            const string test = @"
namespace ParaSmeller.Test.Core
{
    public class A
    {
        public object lockObj = new object();
        public void Method1() 
        {
            int i = 10;
            lock(lockObj) 
            {
                i = 20;
            }
        }
    }

}";
            var solution = TestSolutionBuilder.CreateSolution(test);
            
            var method1 = (solution.ClassMap["ParaSmeller.Test.Core.A"].ToArray()[0].Methods.ToArray()[0]);
            Assert.IsNotNull(method1);
            Assert.AreEqual(1, method1.Blocks.Count);
            Assert.AreEqual(1, method1.Blocks.ToArray()[0].Blocks.Count);
            Assert.IsInstanceOfType(method1.Blocks.ToArray()[0].Blocks.ToArray()[0], typeof(LockBlock));
            Assert.IsInstanceOfType(method1.Blocks.ToArray()[0].Blocks.ToArray()[0].Blocks.ToArray()[0], typeof(NormalBlock));
        }

        [TestMethod]
        public void TestPropertyInvocations()
        {
            const string test = @"
namespace ParaSmeller.Test.Core
{
    public class A
    {
        public string Test 
        {
            get 
            {
                B b = new B();
                b.Method2();
                return ""Test"";
            }
        }
    }

    public class B
    {
        public void Method2() {}
    }

}";
            var solution = TestSolutionBuilder.CreateSolution(test);
            Assert.AreEqual(2, solution.Classes.Count);
            Assert.AreEqual(2, solution.ClassMap.Count);

            var property1 = (solution.ClassMap["ParaSmeller.Test.Core.A"].ToArray()[0].Properties.ToArray()[0]);
            Assert.IsNotNull(property1);
            Assert.AreEqual(1, property1.Blocks.Count);
            Assert.AreEqual(1, property1.Blocks.ToArray()[0].InvocationExpressions.Count);
            var invocationMethodB = property1.Blocks.ToArray()[0].InvocationExpressions.ToArray()[0];
            Assert.AreEqual("ParaSmeller.Test.Core.B", invocationMethodB.CalledClassOriginal);
            Assert.AreEqual("ParaSmeller.Test.Core.B.Method2()", invocationMethodB.OriginalDefinition);
            Assert.AreEqual(1, invocationMethodB.InvokedImplementations.Count);
        }
        
        [TestMethod]
        public void TestInterfaceInvocations()
        {
            const string test = @"
namespace ParaSmeller.Test.Core
{
    public interface A
    {
        public void Method1();
    }

    public class A1 : A
    {
        public override void Method1() {}
    }

    public class A2 : A
    {
        public override void Method1() {}
    }

    public class B 
    {
        public void Method2(A a) 
        {
            a.Method1();
        }
    }

}";
            var solution = TestSolutionBuilder.CreateSolution(test);
            Assert.AreEqual(3, solution.Classes.Count);
            Assert.AreEqual(3, solution.ClassMap.Count);
            Assert.AreEqual(1, solution.InterfaceMap.Count);

            var method2 = (solution.ClassMap["ParaSmeller.Test.Core.B"].ToArray()[0].Methods.ToArray()[0]);
            Assert.IsNotNull(method2);
            Assert.AreEqual(1, method2.Blocks.Count);
            Assert.AreEqual(1, method2.Blocks.ToArray()[0].InvocationExpressions.Count);
            var invocationMethod1 = method2.Blocks.ToArray()[0].InvocationExpressions.ToArray()[0];
            Assert.AreEqual(2, invocationMethod1.InvokedImplementations.Count);

            if (invocationMethod1.InvokedImplementations.ToArray()[0].OriginalDefinition == "ParaSmeller.Test.Core.A2.Method1()")
            {
                Assert.AreEqual("ParaSmeller.Test.Core.A2.Method1()", invocationMethod1.InvokedImplementations.ToArray()[0].OriginalDefinition);
                Assert.AreEqual("ParaSmeller.Test.Core.A1.Method1()", invocationMethod1.InvokedImplementations.ToArray()[1].OriginalDefinition);
            }
            else
            {
                Assert.AreEqual("ParaSmeller.Test.Core.A1.Method1()", invocationMethod1.InvokedImplementations.ToArray()[0].OriginalDefinition);
                Assert.AreEqual("ParaSmeller.Test.Core.A2.Method1()", invocationMethod1.InvokedImplementations.ToArray()[1].OriginalDefinition);
            }
        }


        [TestMethod]
        public void TestInvocationGenericNameSyntax()
        {
            const string test = @"
namespace ParaSmeller.Test.Core
{
    public class A 
    {
        public void Method1() 
        {
            var string1 = ""test"";
            var string2 = ""test2"";
            Swap<string>(ref string1, ref string2);
        }

        void Swap<T>(ref T lhs, ref T rhs)
        {
            T temp;
            temp = lhs;
            lhs = rhs;
            rhs = temp;
        }
    }

}";
            var solution = TestSolutionBuilder.CreateSolution(test);
            
            var method1 = (solution.ClassMap["ParaSmeller.Test.Core.A"].ToArray()[0].Methods.First(s => s.OriginalDefinition == "ParaSmeller.Test.Core.A.Method1()"));
            Assert.IsNotNull(method1);
            var methodSwap = method1.Blocks.ToArray()[0].InvocationExpressions.ToArray()[0];
            Assert.AreEqual(1, methodSwap.InvokedImplementations.Count);
            Assert.AreEqual("ParaSmeller.Test.Core.A.Swap<T>(ref T, ref T)", methodSwap.OriginalDefinition);
        }


        [TestMethod]
        public void TestConditionalAccessExpression()
        {
            const string test = @"
namespace ParaSmeller.Test.Core
{
    public class A
    {
        public void Method1() 
        {
            B b = new B();
            b?.Method2();
        }
    }

    public class B
    {
        public void Method2() {}
    }

}";
            var solution = TestSolutionBuilder.CreateSolution(test);
            var method1 = (solution.ClassMap["ParaSmeller.Test.Core.A"].ToArray()[0].Methods.ToArray()[0]);
            var invocationMethodB = method1.Blocks.ToArray()[0].InvocationExpressions.ToArray()[0];
            Assert.AreEqual("ParaSmeller.Test.Core.B", invocationMethodB.CalledClassOriginal);
            Assert.AreEqual("ParaSmeller.Test.Core.B.Method2()", invocationMethodB.OriginalDefinition);
            Assert.AreEqual(1, invocationMethodB.InvokedImplementations.Count);
        }

        [TestMethod]
        public void TestNestedInvocationExpressionSyntax()
        {
            const string test = @"
namespace ParaSmeller.Test.Core
{
    public class A
    {
        public void Method1() 
        {
            Task.Run(() => { Method2(); });
        }

        private void Method2() {     }
    }

}";
            var solution = TestSolutionBuilder.CreateSolution(test);
            var method1 = (solution.ClassMap["ParaSmeller.Test.Core.A"].ToArray()[0].Methods.First(s => s.OriginalDefinition == "ParaSmeller.Test.Core.A.Method1()"));
            var invocationMethodB = method1.Blocks.ToArray()[0].InvocationExpressions.ToArray()[0];
            Assert.AreEqual(1, invocationMethodB.InvokedImplementations.Count);
            Assert.AreEqual("ParaSmeller.Test.Core.A.Method2()", invocationMethodB.InvokedImplementations.ToArray()[0].OriginalDefinition);
        }




    }
}
