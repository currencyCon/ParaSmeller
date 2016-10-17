using System.Collections.Immutable;
using System.Linq;
using ConcurrencyAnalyzer.Representation;
using ConcurrencyAnalyzer.RepresentationExtensions;
using ConcurrencyAnalyzer.RepresentationFactories;
using ConcurrencyAnalyzer.SyntaxFilters;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ConcurrencyChecker.MonitorWaitOrSignal
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MonitorWaitOrSignalAnalyzer : DiagnosticAnalyzer
    {
        private const string Category = "Synchronization";
        public static string MonitorIfConditionDiagnosticId = "MWS001";
        public static string MonitorPulseDiagnosticId = "MWS002";
        public static string MonitorWaitDefinition = "System.Threading.Monitor.Wait(object)";
        public static string MonitorPulseDefinition = "System.Threading.Monitor.Pulse(object)";

        private static readonly LocalizableString Title =
            new LocalizableResourceString(nameof(Resources.MWSAnalyzerTitle), Resources.ResourceManager,
                typeof (Resources));

        private static readonly LocalizableString MessageFormatIf =
            new LocalizableResourceString(nameof(Resources.MWSIfAnalyzerMessageFormat), Resources.ResourceManager,
                typeof (Resources));

        private static readonly LocalizableString MessageFormatPulse =
            new LocalizableResourceString(nameof(Resources.MWSPulseAnalyzerMessageFormat), Resources.ResourceManager,
                typeof (Resources));

        private static readonly LocalizableString Description =
            new LocalizableResourceString(nameof(Resources.MWSAnalyzerDescription), Resources.ResourceManager,
                typeof (Resources));

        private static readonly DiagnosticDescriptor MonitorIfRule =
            new DiagnosticDescriptor(MonitorIfConditionDiagnosticId, Title, MessageFormatIf, Category,
                DiagnosticSeverity.Warning, true, Description);

        private static readonly DiagnosticDescriptor MonitorPulseRule =
            new DiagnosticDescriptor(MonitorPulseDiagnosticId, Title, MessageFormatPulse, Category,
                DiagnosticSeverity.Warning, true, Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(MonitorIfRule, MonitorPulseRule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationAction(CheckForWrongMonitorUsage);
        }

        private static void CheckForWrongMonitorUsage(CompilationAnalysisContext context)
        {
            var solutionModel = SolutionRepresentationFactory.Create(context.Compilation);

            foreach (var clazz in solutionModel.Classes)
            {
                foreach (var method in clazz.SynchronizedMethods)
                {
                    CheckMonitor(method, context);
                }
            }
        }

        private static void CheckMonitor(IMethodRepresentation method, CompilationAnalysisContext context)
        {
            foreach (var lockStatementSyntax in method.GetLockStatements())
            {
                CheckWaitingCondition(context, lockStatementSyntax);
                CheckPulse(context, lockStatementSyntax);
            }
        }

        private static void CheckPulse(CompilationAnalysisContext context, LockStatementSyntax lockStatementSyntax)
        {
            foreach (
                var monitorPulseExpression in
                    lockStatementSyntax.GetInvocationExpression("Monitor", "Pulse"))
            {
                var test = context.Compilation.GetSemanticModel(monitorPulseExpression.SyntaxTree);
                var symbol = test.GetSymbolInfo(monitorPulseExpression).Symbol;

                var diagn = Diagnostic.Create(MonitorPulseRule, monitorPulseExpression.Parent.GetLocation());
                context.ReportDiagnostic(diagn);
            }
        }

        private static void CheckWaitingCondition(CompilationAnalysisContext context,
            LockStatementSyntax lockStatementSyntax)
        {
            foreach (
                var monitorWaitExpression in
                    lockStatementSyntax.GetInvocationExpression("Monitor", "Wait"))
            {
                var block = monitorWaitExpression.AncestorsAndSelf().OfType<BlockSyntax>().First();
                if (!(block.Parent is WhileStatementSyntax))
                {
                    var diagn = Diagnostic.Create(MonitorIfRule, block.Parent.GetLocation());
                    context.ReportDiagnostic(diagn);
                }
            }
        }
    }
}