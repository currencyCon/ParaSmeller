using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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

            Assert.AreEqual(2, solution.InterfaceMap["ParaSmeller.Test.Core.IBase"].ImplementingClasses.Count);
            Assert.AreEqual(1, solution.InterfaceMap["ParaSmeller.Test.Core.ISub"].ImplementingClasses.Count);

            Assert.AreEqual(1, solution.ClassMap["ParaSmeller.Test.Core.Sub"].ToArray()[0].ClassMap.Count);

            //Assert.AreEqual(2, solution.Classes.ToArray()[0].ClassMap.Count);
            //Assert.AreEqual(1, solution.Classes.ToArray()[1].ClassMap.Count);

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
        public void TestInvocations()
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

        }


    }
}