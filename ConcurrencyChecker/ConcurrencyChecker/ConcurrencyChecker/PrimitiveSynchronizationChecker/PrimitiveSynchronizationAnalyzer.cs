using System.Collections.Immutable;
using System.Linq;
using ConcurrencyAnalyzer.Representation;
using ConcurrencyAnalyzer.RepresentationExtensions;
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
        public const string PrimitiveSynchronizationDiagnosticId = "PS001";
        private const string InterlockedKeyword = "Interlocked";
        private const string VolatileKeyWord = "volatile";
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.PSAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        public static readonly LocalizableString MessageFormatPrimitiveSynchronization = new LocalizableResourceString(nameof(Resources.PrimitiveSynchronizationAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.PSAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private static readonly DiagnosticDescriptor PrimitiveSynchronizationUsageRule = new DiagnosticDescriptor(PrimitiveSynchronizationDiagnosticId, Title, MessageFormatPrimitiveSynchronization, Category, DiagnosticSeverity.Warning, true, Description);

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
                var fieldDeclarations = clazz.ClassDeclarationSyntax.GetChildren<FieldDeclarationSyntax>();
                foreach (var fieldDeclarationSyntax in fieldDeclarations)
                {
                    CheckForVolatileDeclaration(fieldDeclarationSyntax, context);
                }
                foreach (var method in clazz.Methods)
                {
                    CheckForInterlockedUsage(method, context);
                }

            }
        }

        private static void CheckForVolatileDeclaration(FieldDeclarationSyntax fieldDeclarationSyntax, CompilationAnalysisContext context)
        {

            if (fieldDeclarationSyntax.Modifiers.Select(e => e.Text).Contains(VolatileKeyWord))
            {
                ReportPrimitiveSynchronizationUsage(context, fieldDeclarationSyntax);
            }
        }

        private static void CheckForInterlockedUsage(IMember method, CompilationAnalysisContext context)
        {
            var interlockedUsages =
                method.GetChildren<MemberAccessExpressionSyntax>()
                    .Where(e => e.Expression.ToString() == InterlockedKeyword);
            foreach (var interlockedUsage in interlockedUsages)
            {
                ReportPrimitiveSynchronizationUsage(context, interlockedUsage);
            }
        }

        private static void ReportPrimitiveSynchronizationUsage(CompilationAnalysisContext context, SyntaxNode syntaxnode)
        {
            var diagn = Diagnostic.Create(PrimitiveSynchronizationUsageRule, syntaxnode.GetLocation());
            context.ReportDiagnostic(diagn);
        }
    }
}
