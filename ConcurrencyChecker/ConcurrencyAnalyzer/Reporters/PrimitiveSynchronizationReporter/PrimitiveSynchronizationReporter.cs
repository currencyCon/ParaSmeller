using System.Collections.Generic;
using System.Linq;
using ConcurrencyAnalyzer.Representation;
using ConcurrencyAnalyzer.RepresentationExtensions;
using ConcurrencyAnalyzer.SyntaxNodeUtils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Diagnostic = ConcurrencyAnalyzer.Diagnostics.Diagnostic;

namespace ConcurrencyAnalyzer.Reporters.PrimitiveSynchronizationReporter
{
    public class PrimitiveSynchronizationReporter: IReporter
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


        public ICollection<Diagnostic> Report(SolutionRepresentation solutionRepresentation)
        {
            var reports = new List<Diagnostic>();
            foreach (var clazz in solutionRepresentation.Classes)
            {
                var fieldDeclarations = clazz.Implementation.GetChildren<FieldDeclarationSyntax>();
                foreach (var fieldDeclarationSyntax in fieldDeclarations)
                {
                    CheckForUnallowedDeclaration(fieldDeclarationSyntax, reports);
                }
                foreach (var member in clazz.Members)
                {
                    CheckForNotAllowedApiUsages(member, reports);
                }
            }
            return reports;
        }


        private static void CheckForNotAllowedApiUsages(IMember member, ICollection<Diagnostic> reports)
        {
            var invocationsToReport = member.InvocationExpressions.Where(e => NotAllowedApIs.Contains(e.OriginalDefinition));
            foreach (var invocationToReport in invocationsToReport)
            {
                reports.Add(ReportPrimitiveSynchronizationDiagnostic(invocationToReport.Implementation));
            }
            var accessesToReport = member.GetChildren<MemberAccessExpressionSyntax>().Where(e => NotAllowedApiClasses.Contains(e.Expression.ToString()));
            foreach (var accessToReport in accessesToReport)
            {
                reports.Add(ReportPrimitiveSynchronizationDiagnostic(accessToReport));
            }
        }

        private static void CheckForUnallowedDeclaration(BaseFieldDeclarationSyntax fieldDeclarationSyntax, ICollection<Diagnostic> reports)
        {
            if (NotAllowedTypes.Contains(fieldDeclarationSyntax.Declaration.Type.ToString()) ||fieldDeclarationSyntax.Modifiers.Any(e => NotAllowedModifiers.Contains(e.Text)))
            {
                reports.Add(ReportPrimitiveSynchronizationDiagnostic(fieldDeclarationSyntax));
            }
        }

        private static Diagnostic ReportPrimitiveSynchronizationDiagnostic(SyntaxNode syntaxnode)
        {
            return new Diagnostic(PrimitiveSynchronizationDiagnosticId, Title, MessageFormatPrimitiveSynchronization, Description, Category, syntaxnode.GetLocation());
        }
    }
}
