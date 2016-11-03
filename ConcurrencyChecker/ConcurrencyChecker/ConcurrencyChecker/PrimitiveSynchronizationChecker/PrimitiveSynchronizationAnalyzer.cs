using System.Collections.Immutable;
using ConcurrencyAnalyzer.Diagnostics;
using ConcurrencyAnalyzer.Reporter;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Diagnostic = Microsoft.CodeAnalysis.Diagnostic;

namespace ConcurrencyChecker.PrimitiveSynchronizationChecker
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class PrimitiveSynchronizationAnalyzer : DiagnosticAnalyzer
    {
        private const string Category = "Synchronization";
        public const string PrimitiveSynchronizationDiagnosticId = "PS001";

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
            var smellReporter = new SmellReporter();
            var diags = await smellReporter.Report(context.Compilation, Smell.PrimitiveSynchronization);
            foreach (var diagnostic in diags)
            {
                context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(diagnostic.Id, diagnostic.Title, diagnostic.MessageFormat, diagnostic.Category, DiagnosticSeverity.Warning, true, diagnostic.Description), diagnostic.Location));
            }
        }
    }
}
