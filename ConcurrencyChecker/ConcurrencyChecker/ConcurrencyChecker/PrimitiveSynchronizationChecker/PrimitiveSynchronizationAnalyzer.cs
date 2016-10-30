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
        private const string YieldOriginalDefinition = "System.Threading.Thread.Yield";
        private const string MemoryBarrierOriginalDefinition = "System.Threading.Thread.MemoryBarrier";
        private const string SpinLockExitOriginalDefinition = "System.Threading.SpinLock.Exit";
        private const string SpinLockEnterOriginalDefinition = "System.Threading.SpinLock.Enter";
        private const string SpinLockType = "SpinLock";
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
                    CheckForSpinLockDeclaration(fieldDeclarationSyntax, context);
                }
                foreach (var member in clazz.Members)
                {
                    CheckForInterlockedUsage(member, context);
                    CheckForYieldUsage(member, context);
                    CheckForSpinLockUsage(member, context);
                    CheckForMemoryBarrierUsage(member, context);
                }

            }
        }

        private static void CheckForMemoryBarrierUsage(IMember member, CompilationAnalysisContext context)
        {
            var memoryBarrierUsages = member.InvocationExpressions.Where(e => e.OriginalDefinition == MemoryBarrierOriginalDefinition);
            foreach (var memoryBarrierUsage in memoryBarrierUsages)
            {
                ReportPrimitiveSynchronizationUsage(context, memoryBarrierUsage.Implementation);
            }
        }

        private static void CheckForSpinLockDeclaration(BaseFieldDeclarationSyntax fieldDeclarationSyntax, CompilationAnalysisContext context)
        {
            if (fieldDeclarationSyntax.Declaration.Type.ToString() == SpinLockType)
            {
                ReportPrimitiveSynchronizationUsage(context, fieldDeclarationSyntax);
            }
        }

        private static void CheckForSpinLockUsage(IMember method, CompilationAnalysisContext context)
        {
            var spinlockUsages = method.InvocationExpressions.Where(e => e.OriginalDefinition == SpinLockEnterOriginalDefinition || e.OriginalDefinition == SpinLockExitOriginalDefinition);
            foreach (var interlockedUsage in spinlockUsages)
            {
                ReportPrimitiveSynchronizationUsage(context, interlockedUsage.Implementation);
            }
        }

        private static void CheckForYieldUsage(IMember member, CompilationAnalysisContext context)
        {
            var yieldUsages = member.InvocationExpressions.Where(e => e.OriginalDefinition == YieldOriginalDefinition);
            foreach (var yieldUsage in yieldUsages)
            {
                ReportPrimitiveSynchronizationUsage(context, yieldUsage.Implementation);
            }
        }

        private static void CheckForVolatileDeclaration(BaseFieldDeclarationSyntax fieldDeclarationSyntax, CompilationAnalysisContext context)
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
