using System.Collections.Immutable;
using ConcurrencyAnalyzer.Diagnostics;
using ConcurrencyAnalyzer.Reporters;
using ConcurrencyAnalyzer.Reporters.PrimitiveSynchronizationReporter;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Diagnostic = Microsoft.CodeAnalysis.Diagnostic;

namespace ConcurrencyChecker.PrimitiveSynchronizationChecker
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class PrimitiveSynchronizationAnalyzer : DiagnosticAnalyzer
    {

        private static readonly DiagnosticDescriptor PrimitiveSynchronizationUsageRule = new DiagnosticDescriptor(PrimitiveSynchronizationReporter.PrimitiveSynchronizationDiagnosticId, PrimitiveSynchronizationReporter.Title, PrimitiveSynchronizationReporter.MessageFormatPrimitiveSynchronization, PrimitiveSynchronizationReporter.Category, DiagnosticSeverity.Warning, true, PrimitiveSynchronizationReporter.Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(PrimitiveSynchronizationUsageRule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationAction(CheckForPrimitiveSynchronization);
        }

        private static async void CheckForPrimitiveSynchronization(CompilationAnalysisContext context)
        {
            var smellReporter = new SmellReporter();
            var diagnostics = await smellReporter.Report(context.Compilation, Smell.PrimitiveSynchronization);
            foreach (var diagnostic in diagnostics)
            {
                context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(diagnostic.Id, diagnostic.Title, diagnostic.MessageFormat, diagnostic.Category, DiagnosticSeverity.Warning, true, diagnostic.Description), diagnostic.Location));
            }
        }
    }
}
