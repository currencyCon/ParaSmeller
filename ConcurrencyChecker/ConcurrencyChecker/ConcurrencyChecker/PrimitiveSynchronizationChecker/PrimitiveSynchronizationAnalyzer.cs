using System.Collections.Immutable;
using System.Linq;
using ConcurrencyAnalyzer.Representation;
using ConcurrencyAnalyzer.RepresentationFactories;
using ConcurrencyAnalyzer.SyntaxFilters;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ConcurrencyChecker.PrimitiveSynchronizationChecker
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class PrimitiveSynchronizationAnalyzer: DiagnosticAnalyzer
    {
        private const string Category = "Synchronization";
        public const string InterlockedUsageDiagnosticId = "PS001";
        private const string InterlockedKeyword = "Interlocked";
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.PSAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        public static readonly LocalizableString MessageFormatInterlockedUsage = new LocalizableResourceString(nameof(Resources.InterlockedUsageAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.PSAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private static readonly DiagnosticDescriptor InterlockedUsageRule = new DiagnosticDescriptor(InterlockedUsageDiagnosticId, Title, MessageFormatInterlockedUsage, Category, DiagnosticSeverity.Warning, true, Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(InterlockedUsageRule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationAction(CheckForPrimitiveSynchronization);
        }

        private static async void CheckForPrimitiveSynchronization(CompilationAnalysisContext context)
        {
            var solutionModel = await SolutionRepresentationFactory.Create(context.Compilation);

            foreach (var clazz in solutionModel.Classes)
            {
                foreach (var method in clazz.Methods)
                {
                    CheckForInterlockedUsage(method, context);
                }

            }
        }

        private static void CheckForInterlockedUsage(MethodRepresentation method, CompilationAnalysisContext context)
        {
            var interlockedUsages =
                method.MethodImplementation.GetChildren<MemberAccessExpressionSyntax>()
                    .Where(e => e.Expression.ToString() == InterlockedKeyword);
            foreach (var interlockedUsage in interlockedUsages)
            {
                ReportInterlockedUsage(context, interlockedUsage);
            }
        }

        private static void ReportInterlockedUsage(CompilationAnalysisContext context, SyntaxNode interlockedExpression)
        {
            var diagn = Diagnostic.Create(InterlockedUsageRule, interlockedExpression.GetLocation());
            context.ReportDiagnostic(diagn);
        }
    }
}
