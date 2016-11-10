using System.Linq;
using ConcurrencyAnalyzer.Representation;
using ConcurrencyAnalyzer.RepresentationExtensions;
using ConcurrencyAnalyzer.SyntaxNodeUtils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Diagnostic = ConcurrencyAnalyzer.Diagnostics.Diagnostic;

namespace ConcurrencyAnalyzer.Reporters
{
    public class PrimitiveSynchronizationReporter: BaseReporter
    {
        public const string Category = "Synchronization";
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

        public static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.PSAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        public static readonly LocalizableString MessageFormatPrimitiveSynchronization = new LocalizableResourceString(nameof(Resources.PrimitiveSynchronizationAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        public static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.PSAnalyzerDescription), Resources.ResourceManager, typeof(Resources));

        private void CheckForUnallowedDeclaration(ClassRepresentation clazz)
        {
            var fieldDeclarations = clazz.Implementation.GetChildren<FieldDeclarationSyntax>();
            foreach (var fieldDeclarationSyntax in fieldDeclarations)
            {
                CheckForUnallowedDeclaration(fieldDeclarationSyntax);
            }
        }

        private void CheckForNotAllowedApiUsages(Member member)
        {
            var invocationsToReport = member.GetAllInvocations().Where(e => NotAllowedApIs.Contains(e.OriginalDefinition));
            foreach (var invocationToReport in invocationsToReport)
            {
                Reports.Add(ReportPrimitiveSynchronizationDiagnostic(invocationToReport.Implementation));
            }
            var accessesToReport = member.GetChildren<MemberAccessExpressionSyntax>().Where(e => NotAllowedApiClasses.Contains(e.Expression.ToString()));
            foreach (var accessToReport in accessesToReport)
            {
                Reports.Add(ReportPrimitiveSynchronizationDiagnostic(accessToReport));
            }
        }

        private void CheckForUnallowedDeclaration(BaseFieldDeclarationSyntax fieldDeclarationSyntax)
        {
            if (NotAllowedTypes.Contains(fieldDeclarationSyntax.Declaration.Type.ToString()) ||fieldDeclarationSyntax.Modifiers.Any(e => NotAllowedModifiers.Contains(e.Text)))
            {
                Reports.Add(ReportPrimitiveSynchronizationDiagnostic(fieldDeclarationSyntax));
            }
        }

        private static Diagnostic ReportPrimitiveSynchronizationDiagnostic(SyntaxNode syntaxnode)
        {
            return new Diagnostic(PrimitiveSynchronizationDiagnosticId, Title, MessageFormatPrimitiveSynchronization, Description, Category, syntaxnode.GetLocation());
        }
        public override void Register()
        {
            RegisterMemberReport(CheckForNotAllowedApiUsages);
            RegisterClassReport(CheckForUnallowedDeclaration);
        }
    }
}
