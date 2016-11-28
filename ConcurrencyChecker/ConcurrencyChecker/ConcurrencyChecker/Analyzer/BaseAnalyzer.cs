using System.Collections.Generic;
using System.Linq;
using ConcurrencyAnalyzer;
using ConcurrencyAnalyzer.Diagnostics;
using ConcurrencyAnalyzer.Reporters;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Diagnostic = Microsoft.CodeAnalysis.Diagnostic;

namespace ConcurrencyChecker.Analyzer
{
    public abstract class BaseAnalyzer: DiagnosticAnalyzer
    {
        protected static void ReportDiagnostics(CompilationAnalysisContext context, ICollection<ConcurrencyAnalyzer.Diagnostics.Diagnostic> diagnostics)
        {
            Logger.Debug($"Found {diagnostics.Count} diagnostics");
            foreach (var diagnostic in diagnostics)
            {
                var diag = new DiagnosticDescriptor(diagnostic.Id, diagnostic.Title, diagnostic.MessageFormat,
                    diagnostic.Category, DiagnosticSeverity.Warning, true, diagnostic.Description);
                if (diagnostic.Parameter != null)
                {
                    context.ReportDiagnostic(Diagnostic.Create(diag, diagnostic.Location, diagnostic.Parameter));
                }
                else
                {
                    context.ReportDiagnostic(Diagnostic.Create(diag, diagnostic.Location));
                }
                Logger.Debug($"DIAG::: {diagnostic.Id}: {diagnostic.Description}, {diagnostic.MessageFormat}, Location:{diagnostic.Location}");
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationAction(RegisterDiagnostics);
        }

        private void RegisterDiagnostics(CompilationAnalysisContext context)
        {
            Logger.Debug("Starting Analyzer");
            if (SelectSmell().Any())
            {
                RegisterDiagnostics(context, SelectSmell());
            }
            else
            {
                RegisterDiagnostics(context, SmellReporter.DefaultSmellCollection);
            }
        }

        private static async void RegisterDiagnostics(CompilationAnalysisContext context, ICollection<Smell> smells)
        {
            var smellReporter = new SmellReporter();
            var diagnostics = await smellReporter.Report(context.Compilation, smells);
            ReportDiagnostics(context, diagnostics);
        }

        protected virtual ICollection<Smell> SelectSmell()
        {
            return SmellReporter.DefaultSmellCollection;
        }
    }
}
