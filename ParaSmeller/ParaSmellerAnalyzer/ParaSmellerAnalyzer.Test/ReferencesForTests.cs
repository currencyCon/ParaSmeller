using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParaSmellerAnalyzer.Analyzer;
using CodeFixVerifier = ParaSmeller.Test.Verifiers.CodeFixVerifier;

namespace ParaSmeller.Test
{

    [DeploymentItem("bin\\debug\\Microsoft.Build.dll")]
    [DeploymentItem("bin\\debug\\Microsoft.Build.Engine.dll")]
    [DeploymentItem("bin\\debug\\Microsoft.Build.Framework.dll")]
    [DeploymentItem("bin\\debug\\Microsoft.Build.Tasks.Core.dll")]
    [DeploymentItem("bin\\debug\\Microsoft.Build.Utilities.Core.dll")]
    [DeploymentItem("bin\\debug\\Microsoft.CodeAnalysis.CSharp.dll")]
    [DeploymentItem("bin\\debug\\Microsoft.CodeAnalysis.CSharp.Workspaces.dll")]
    [DeploymentItem("bin\\debug\\Microsoft.CodeAnalysis.dll")]
    [DeploymentItem("bin\\debug\\Microsoft.CodeAnalysis.VisualBasic.dll")]
    [DeploymentItem("bin\\debug\\Microsoft.CodeAnalysis.VisualBasic.Workspaces.dll")]
    [DeploymentItem("bin\\debug\\Microsoft.CodeAnalysis.Workspaces.Desktop.dll")]
    [DeploymentItem("bin\\debug\\Microsoft.CodeAnalysis.Workspaces.dll")]
    [DeploymentItem("bin\\debug\\System.Collections.Immutable.dll")]
    [DeploymentItem("bin\\debug\\System.Composition.AttributedModel.dll")]
    [DeploymentItem("bin\\debug\\System.Composition.Convention.dll")]
    [DeploymentItem("bin\\debug\\System.Composition.Hosting.dll")]
    [DeploymentItem("bin\\debug\\System.Composition.Runtime.dll")]
    [DeploymentItem("bin\\debug\\System.Composition.TypedParts.dll")]
    [DeploymentItem("bin\\debug\\System.Reflection.Metadata.dll")]
    [TestClass]
    public class ReferencesForTests : CodeFixVerifier
    {



        [TestMethod]
        public void ReferenceTest()
        {
           Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestWithInterface()
        {
            string test = @"
using System;

namespace ConsoleApplication3
{
    public class HausKatze : IKatze
    {
        public void Laufen()
        {
            throw new NotImplementedException();
        }
    }


    public class NormaleKatze : IKatze
    {
        public void Laufen()
        {
            throw new NotImplementedException();
        }
    }
    public interface IKatze
    {
        void Laufen();
    }

    public class Main
    {
        public void Test(IKatze katze)
        {
            katze.Laufen();
        }
    }
}


";
            VerifyCSharpDiagnostic(test);
        }


        [TestMethod]
        public void TestWithAbstractClass()
        {
            string test = @"
using System;

namespace ConsoleApplication3
{
    public class HausKatze : IKatze
    {
        public override void Laufen()
        {
            throw new NotImplementedException();
        }
    }


    public class NormaleKatze : IKatze
    {
        public override void Laufen()
        {
            throw new NotImplementedException();
        }
    }
    public abstract class IKatze
    {
        public abstract void Laufen();
    }

    public class Main
    {
        public void Test(IKatze katze)
        {
            katze.Laufen();
        }
    }
}


";
            VerifyCSharpDiagnostic(test);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new AllAnalyzer();
        }

    }
}