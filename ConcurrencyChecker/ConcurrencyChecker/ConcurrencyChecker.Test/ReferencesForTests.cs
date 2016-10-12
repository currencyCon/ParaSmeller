using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConcurrencyChecker.Test
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
    public class ReferencesForTests
    {

        [TestMethod]
        public void ReferenceTest()
        {
           Assert.IsTrue(true);
        }
        
    }
}