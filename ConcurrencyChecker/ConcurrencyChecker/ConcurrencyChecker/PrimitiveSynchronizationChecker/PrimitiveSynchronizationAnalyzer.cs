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
    public class PrimitiveSynchronizationAnalyzer : DiagnosticAnalyzer
    {
        private const string Category = "Synchronization";
        public const string PrimitiveSynchronizationDiagnosticId = "PS001";
        private const string InterlockedKeyword = "Interlocked";
        private const string VolatileKeyWord = "volatile";
        private const string YieldOriginalDefinition = "System.Threading.Thread.Yield";
        private const string MemoryBarrierOriginalDefinition = "System.Threading.Thread.MemoryBarrier";
        private const string SpinLockExitOriginalDefinition = "System.Threading.SpinLock.Exit";
        private const string SpinLockEnterOriginalDefinition = "System.Threading.SpinLock.Enter";
        private const string SpinLockType = "SpinLock";

        private static readonly string[] NotAllowedApIs = {YieldOriginalDefinition, MemoryBarrierOriginalDefinition, SpinLockEnterOriginalDefinition, SpinLockExitOriginalDefinition };
        private static readonly string[] NotAllowedTypes = { SpinLockType };
        private static readonly string[] NotAllowedModifiers = { VolatileKeyWord };
        private static readonly string[] NotAllowedApiClasses = { InterlockedKeyword };

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
                var fieldDeclarations = clazz.Implementation.GetChildren<FieldDeclarationSyntax>();
                foreach (var fieldDeclarationSyntax in fieldDeclarations)
                {
                    CheckForUnallowedDeclaration(fieldDeclarationSyntax, context);
                }
                foreach (var member in clazz.Members)
                {
                    CheckForNotAllowedApiUsages(member, context);
                }
            }
        }

        private static void CheckForNotAllowedApiUsages(IMember member, CompilationAnalysisContext context)
        {
            var invocationsToReport = member.InvocationExpressions.Where(e => NotAllowedApIs.Contains(e.OriginalDefinition));
            foreach (var invocationToReport in invocationsToReport)
            {
                ReportPrimitiveSynchronizationUsage(context, invocationToReport.Implementation);
            }
            var accessesToReport = member.GetChildren<MemberAccessExpressionSyntax>().Where(e => NotAllowedApiClasses.Contains(e.Expression.ToString()));
            foreach (var accessToReport in accessesToReport)
            {
                ReportPrimitiveSynchronizationUsage(context, accessToReport);
            }
        }

        private static void CheckForUnallowedDeclaration(BaseFieldDeclarationSyntax fieldDeclarationSyntax, CompilationAnalysisContext context)
        {
            if (NotAllowedTypes.Contains(fieldDeclarationSyntax.Declaration.Type.ToString()) ||fieldDeclarationSyntax.Modifiers.Any(e => NotAllowedModifiers.Contains(e.Text)))
            {
                ReportPrimitiveSynchronizationUsage(context, fieldDeclarationSyntax);
            }
        }

        private static void ReportPrimitiveSynchronizationUsage(CompilationAnalysisContext context, SyntaxNode syntaxnode)
        {
            var diagn = Diagnostic.Create(PrimitiveSynchronizationUsageRule, syntaxnode.GetLocation());
            context.ReportDiagnostic(diagn);
        }
    }
}
