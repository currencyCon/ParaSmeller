using System.Collections.Immutable;
using ConcurrencyAnalyzer.Diagnostics;
using ConcurrencyAnalyzer.Reporters;
using ConcurrencyAnalyzer.Reporters.FinalizerReporter;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Diagnostic = Microsoft.CodeAnalysis.Diagnostic;

namespace ConcurrencyChecker.FinalizerChecker
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class FinalizerSynchronizationAnalyzer: DiagnosticAnalyzer
    {

        private static readonly DiagnosticDescriptor FinalizerSynchronizationUsageRule = new DiagnosticDescriptor(FinalizerReporter.FinalizerSynchronizationDiagnosticId, FinalizerReporter.Title, FinalizerReporter.MessageFormatFinalizerSynchronization, FinalizerReporter.Category, DiagnosticSeverity.Warning, true, FinalizerReporter.Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(FinalizerSynchronizationUsageRule);
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationAction(CheckForUnsynchronizedFinalizers);
        }

        private static async void CheckForUnsynchronizedFinalizers(CompilationAnalysisContext context)
        {
            var smellReporter = new SmellReporter();
            var diagnostics = await smellReporter.Report(context.Compilation, Smell.Finalizer);
            foreach (var diagnostic in diagnostics)
            {
                context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(diagnostic.Id, diagnostic.Title, diagnostic.MessageFormat, diagnostic.Category, DiagnosticSeverity.Warning, true, diagnostic.Description), diagnostic.Location));
            }
        }
    }
}
