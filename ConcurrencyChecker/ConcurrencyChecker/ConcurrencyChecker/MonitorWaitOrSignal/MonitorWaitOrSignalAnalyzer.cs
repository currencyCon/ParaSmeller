using System.Collections.Immutable;
using System.Linq;
using ConcurrencyAnalyzer.Representation;
using ConcurrencyAnalyzer.RepresentationExtensions;
using ConcurrencyAnalyzer.RepresentationFactories;
using ConcurrencyAnalyzer.SyntaxNodeUtils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ConcurrencyChecker.MonitorWaitOrSignal
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MonitorWaitOrSignalAnalyzer : DiagnosticAnalyzer
    {
        private const string Category = "Synchronization";
        public const string MonitorIfConditionDiagnosticId = "MWS001";
        public const string MonitorPulseDiagnosticId = "MWS002";
        public const string MonitorWaitDefinition = "System.Threading.Monitor.Wait(object)";
        public const string MonitorPulseDefinition = "System.Threading.Monitor.Pulse(object)";
        private const string MonitorClass = "Monitor";
        private const string MonitorPulseMethod = "Pulse";
        private const string MonitorWaitMethod = "Wait";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.MWSAnalyzerTitle), Resources.ResourceManager, typeof (Resources));
        private static readonly LocalizableString MessageFormatIf = new LocalizableResourceString(nameof(Resources.MWSIfAnalyzerMessageFormat), Resources.ResourceManager, typeof (Resources));
        private static readonly LocalizableString MessageFormatPulse = new LocalizableResourceString(nameof(Resources.MWSPulseAnalyzerMessageFormat), Resources.ResourceManager, typeof (Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.MWSAnalyzerDescription), Resources.ResourceManager, typeof (Resources));
        private static readonly DiagnosticDescriptor MonitorIfRule = new DiagnosticDescriptor(MonitorIfConditionDiagnosticId, Title, MessageFormatIf, Category, DiagnosticSeverity.Warning, true, Description);
        private static readonly DiagnosticDescriptor MonitorPulseRule = new DiagnosticDescriptor(MonitorPulseDiagnosticId, Title, MessageFormatPulse, Category, DiagnosticSeverity.Warning, true, Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(MonitorIfRule, MonitorPulseRule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationAction(CheckForWrongMonitorUsage);
        }

        private static async void CheckForWrongMonitorUsage(CompilationAnalysisContext context)
        {
            var solutionModel = await SolutionRepresentationFactory.Create(context.Compilation);

            foreach (var clazz in solutionModel.Classes)
            {
                foreach (var method in clazz.Methods)
                {
                    CheckWaitInsideLock(method, context);
                    CheckWaitOutsideLock(clazz, method, context);
                }

                CheckPulse(context, clazz.Implementation);
            }
        }

        private static void CheckWaitOutsideLock(ClassRepresentation clazz, MethodRepresentation method, CompilationAnalysisContext context)
        {
            foreach (var expressionSyntax in method.Implementation.GetInvocationExpression(MonitorClass, MonitorWaitMethod).Where(e => !e.IsSynchronized()))
            {
                if (expressionSyntax.IsInTopLevelBlock())
                {
                    CheckFunctionCallers(clazz, method, context);
                }
            }
        }

        private static void CheckWaitInsideLock(IMember method, CompilationAnalysisContext context)
        {
            foreach (var monitorWaitExpression in method.GetLockStatements().SelectMany(e => e.GetInvocationExpression(MonitorClass, MonitorWaitMethod)))
            {
                CheckCondition(context, monitorWaitExpression);
            }
        }

        private static void CheckFunctionCallers(ClassRepresentation clazz, IMember method, CompilationAnalysisContext context)
        {
            foreach (var invocationExpression in clazz.Implementation.GetChildren<InvocationExpressionSyntax>().Where(i => i.Expression.ToString() == method.Name.ToString()))
            {
                CheckCondition(context, invocationExpression);
            }
        }

        private static void CheckPulse(CompilationAnalysisContext context, SyntaxNode syntaxNode)
        {
            foreach (var monitorPulseExpression in syntaxNode.GetInvocationExpression(MonitorClass, MonitorPulseMethod))
            {
                var diagn = Diagnostic.Create(MonitorPulseRule, monitorPulseExpression.Parent.GetLocation());
                context.ReportDiagnostic(diagn);
            }
        }

        private static void CheckCondition(CompilationAnalysisContext context, SyntaxNode monitorWaitExpression)
        {
            var block = monitorWaitExpression.GetFirstParent<BlockSyntax>();
            if (!(block.Parent is WhileStatementSyntax) && !(block.Parent is DoStatementSyntax))
            {
                var diagn = Diagnostic.Create(MonitorIfRule, block.Parent.GetLocation());
                context.ReportDiagnostic(diagn);
            }
        }
    }
}