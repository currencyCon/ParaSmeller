using System.Collections.Generic;
using System.Linq;
using ConcurrencyAnalyzer.Representation;
using ConcurrencyAnalyzer.RepresentationExtensions;
using ConcurrencyAnalyzer.SyntaxNodeUtils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Diagnostic = ConcurrencyAnalyzer.Diagnostics.Diagnostic;

namespace ConcurrencyAnalyzer.Reporters.MonitorOrWaitSignalReporter
{
    public class MonitorOrWaitSignalReporter: IReporter
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

        private static void CheckWaitOutsideLock(ClassRepresentation clazz, MethodRepresentation method, ICollection<Diagnostic> reports)
        {
            foreach (var expressionSyntax in method.Implementation.GetInvocationExpression(MonitorClass, MonitorWaitMethod).Where(e => !e.IsSynchronized()))
            {
                if (expressionSyntax.IsInTopLevelBlock())
                {
                    CheckFunctionCallers(clazz, method, reports);
                }
            }
        }

        private static void CheckWaitInsideLock(IMember method, ICollection<Diagnostic> reports)
        {
            foreach (var monitorWaitExpression in method.GetLockStatements().SelectMany(e => e.GetInvocationExpression(MonitorClass, MonitorWaitMethod)))
            {
                var report = CheckCondition(monitorWaitExpression);
                if (report != null)
                {
                    reports.Add(report);
                }
            }
        }

        private static void CheckFunctionCallers(ClassRepresentation clazz, IMember method, ICollection<Diagnostic> reports)
        {
            foreach (var invocationExpression in clazz.Implementation.GetChildren<InvocationExpressionSyntax>().Where(i => i.Expression.ToString() == method.Name.ToString()))
            {
                var report = CheckCondition(invocationExpression);
                if (report != null)
                {
                    reports.Add(report);
                }
            }
        }

        private static void CheckPulse(ICollection<Diagnostic> reports, SyntaxNode syntaxNode)
        {
            foreach (var monitorPulseExpression in syntaxNode.GetInvocationExpression(MonitorClass, MonitorPulseMethod))
            {
                reports.Add(new Diagnostic(MonitorPulseDiagnosticId, Title, MessageFormatPulse, Description, Category,
                    monitorPulseExpression.Parent.GetLocation()));

            }
        }

        private static Diagnostic CheckCondition(SyntaxNode monitorWaitExpression)
        {
            var block = monitorWaitExpression.GetFirstParent<BlockSyntax>();
            if (!(block.Parent is WhileStatementSyntax) && !(block.Parent is DoStatementSyntax))
            {
                return new Diagnostic(MonitorIfConditionDiagnosticId, Title, MessageFormatIf, Description, Category,
                    block.Parent.GetLocation());
            }
            return null;
        }
        public ICollection<Diagnostic> Report(SolutionRepresentation solutionRepresentation)
        {
            var reports = new List<Diagnostic>();
            foreach (var clazz in solutionRepresentation.Classes)
            {
                foreach (var method in clazz.Methods)
                {
                    CheckWaitInsideLock(method, reports);
                    CheckWaitOutsideLock(clazz, method, reports);
                }

                CheckPulse(reports, clazz.Implementation);
            }
            return reports;
        }
    }
}
