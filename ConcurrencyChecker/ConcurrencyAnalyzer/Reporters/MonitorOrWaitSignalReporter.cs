using System.Linq;
using ConcurrencyAnalyzer.Representation;
using ConcurrencyAnalyzer.RepresentationExtensions;
using ConcurrencyAnalyzer.SyntaxNodeUtils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Diagnostic = ConcurrencyAnalyzer.Diagnostics.Diagnostic;

namespace ConcurrencyAnalyzer.Reporters
{
    public class MonitorOrWaitSignalReporter: BaseReporter
    {
        public const string Category = "Synchronization";
        public const string MonitorIfConditionDiagnosticId = "MWS001";
        public const string MonitorPulseDiagnosticId = "MWS002";
        private const string MonitorClass = "Monitor";
        private const string MonitorPulseMethod = "Pulse";
        private const string MonitorWaitMethod = "Wait";
          
        public static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.MWSAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        public static readonly LocalizableString MessageFormatIf = new LocalizableResourceString(nameof(Resources.MWSIfAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        public static readonly LocalizableString MessageFormatPulse = new LocalizableResourceString(nameof(Resources.MWSPulseAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        public static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.MWSAnalyzerDescription), Resources.ResourceManager, typeof(Resources));

        private void CheckWaitOutsideLock(MethodRepresentation method)
        {
            foreach (var expressionSyntax in method.Implementation.GetInvocationExpression(MonitorClass, MonitorWaitMethod).Where(e => !e.IsSynchronized()))
            {
                if (expressionSyntax.IsInTopLevelBlock())
                {
                    CheckFunctionCallers(method);
                }
            }
        }

        private void CheckWaitInsideLock(Member method)
        {
            foreach (var monitorWaitExpression in method.GetLockStatements().SelectMany(e => e.GetInvocationExpression(MonitorClass, MonitorWaitMethod)))
            {
                CheckCondition(monitorWaitExpression);
            }
        }

        private void CheckFunctionCallers(Member method)
        {
            foreach (var invocationExpression in method.ContainingClass.Implementation.GetChildren<InvocationExpressionSyntax>().Where(i => i.Expression.ToString() == method.Name.ToString()))
            {
                CheckCondition(invocationExpression);
            }
        }

        private void CheckPulse(ClassRepresentation classRepresentation)
        {
            foreach (var monitorPulseExpression in classRepresentation.Implementation.GetInvocationExpression(MonitorClass, MonitorPulseMethod))
            {
                Reports.Add(new Diagnostic(MonitorPulseDiagnosticId, Title, MessageFormatPulse, Description, Category,
                    monitorPulseExpression.Parent.GetLocation()));

            }
        }

        private void CheckCondition(SyntaxNode monitorWaitExpression)
        {
            var block = monitorWaitExpression.GetFirstParent<BlockSyntax>();
            if (!(block.Parent is WhileStatementSyntax) && !(block.Parent is DoStatementSyntax))
            {
                Reports.Add(new Diagnostic(MonitorIfConditionDiagnosticId, Title, MessageFormatIf, Description, Category,
                    block.Parent.GetLocation()));
            }
        }

        public override void Register()
        {
            RegisterClassReport(CheckPulse);
            RegisterMethodReport(CheckWaitOutsideLock);
            RegisterMethodReport(CheckWaitInsideLock);
        }
    }
}
