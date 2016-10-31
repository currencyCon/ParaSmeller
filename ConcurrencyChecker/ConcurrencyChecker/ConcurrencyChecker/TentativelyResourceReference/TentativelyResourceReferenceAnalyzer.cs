using System.Collections.Immutable;
using System.Linq;
using ConcurrencyAnalyzer.Representation;
using ConcurrencyAnalyzer.RepresentationExtensions;
using ConcurrencyAnalyzer.RepresentationFactories;
using ConcurrencyAnalyzer.SyntaxFilters;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ConcurrencyChecker.TentativelyResourceReference
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TentativelyResourceReferenceAnalyzer : DiagnosticAnalyzer
    {
        private const string Category = "Synchronization";
        public const string DiagnosticId = "TRR001";

        private const string MonitorWait = "System.Threading.Monitor.Wait";
        private const string MonitorTryEnter = "System.Threading.Monitor.TryEnter";
        private const string WaitHandleWaitOne = "System.Threading.WaitHandle.WaitOne";
        private const string WaitHandleWaitAll = "System.Threading.WaitHandle.WaitAll";
        private const string WaitHandleWaitAny = "System.Threading.WaitHandle.WaitAny";
        private const string SpinLockTryEnter = "System.Threading.SpinLock.TryEnter";
        private const string BarrierSignalAndWait = "System.Threading.Barrier.SignalAndWait";

        private const bool CheckNotOnlyInsideWhileLoop = true;
        
        private static readonly string[] TimeoutTypes = { "TimeSpan", "Int32" };
        private static readonly string[] NotAllowedApis = { MonitorWait, WaitHandleWaitOne, WaitHandleWaitAll, WaitHandleWaitAny, MonitorTryEnter, SpinLockTryEnter, BarrierSignalAndWait };
        
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.TRRAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        public static readonly LocalizableString MessageFormatPrimitiveSynchronization = new LocalizableResourceString(nameof(Resources.TRRAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.TRRAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private static readonly DiagnosticDescriptor PrimitiveSynchronizationUsageRule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormatPrimitiveSynchronization, Category, DiagnosticSeverity.Warning, true, Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(PrimitiveSynchronizationUsageRule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationAction(CheckForPrimitiveSynchronization);
        }

        private static async void CheckForPrimitiveSynchronization(CompilationAnalysisContext context)
        {
            var solutionModel = await SolutionRepresentationFactory.Create(context.Compilation);

            foreach (var clazz in solutionModel.Classes)
            {
                foreach (var member in clazz.Methods)
                {
                    CheckForNotAllowedApiUsages(member, context);
                }
            }
        }

        private static void CheckForNotAllowedApiUsages(MethodRepresentation method, CompilationAnalysisContext context)
        {
            var invocationsToReport = method.Blocks.ToList()[0].InvocationExpressions.Where(e => NotAllowedApis.Contains(e.OriginalDefinition));
            foreach (var invocationToReport in invocationsToReport)
            {
                var symbol = invocationToReport.GetMethodSymbol(context);

                if (ContainsTimeout(symbol)/*&& (IsInWhileLoop(invocationToReport.Implementation) || CheckNotOnlyInsideWhileLoop)*/)
                {
                    ReportTimeoutUsage(context, invocationToReport.Implementation);
                }
            }
        }

        private static bool ContainsTimeout(IMethodSymbol methodSymbol)
        {
            var parameters = methodSymbol.Parameters;
            foreach (var parameter in parameters)
            {
                if (TimeoutTypes.Contains(parameter.Type.Name))
                {
                    return true;
                }
            }
            return false;
        }


        private static bool IsInWhileLoop(SyntaxNode node)
        {
            var block = node.GetFirstParent<BlockSyntax>();

            if (block.Parent is WhileStatementSyntax || block.Parent is DoStatementSyntax)
            {
                return true;
            }

            if (node.Parent is WhileStatementSyntax || node.Parent is DoStatementSyntax)
            {
                return true;
            }

            return false;
        }

        private static void ReportTimeoutUsage(CompilationAnalysisContext context, SyntaxNode syntaxnode)
        {
            var diagn = Diagnostic.Create(PrimitiveSynchronizationUsageRule, syntaxnode.GetLocation());
            context.ReportDiagnostic(diagn);
        }
    }
}
