using System.Collections.Generic;
using ConcurrencyAnalyzer.Diagnostics;
using ConcurrencyAnalyzer.Reporters;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Diagnostic = Microsoft.CodeAnalysis.Diagnostic;

namespace ConcurrencyChecker
{
    public abstract class BaseAnalyzer: DiagnosticAnalyzer
    {
        protected static void ReportDiagnostics(CompilationAnalysisContext context, ICollection<ConcurrencyAnalyzer.Diagnostics.Diagnostic> diagnostics)
        {
          
            foreach (var diagnostic in diagnostics)
            {
                var diag = new DiagnosticDescriptor(diagnostic.Id, diagnostic.Title, diagnostic.MessageFormat,
                    diagnostic.Category, DiagnosticSeverity.Warning, true, diagnostic.Description);
                if (diagnostic.Params != null)
                {
                    context.ReportDiagnostic(Diagnostic.Create(diag, diagnostic.Location, diagnostic.Params));
                }
                else
                {
                    context.ReportDiagnostic(Diagnostic.Create(diag, diagnostic.Location));
                }
                
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationAction(RegisterDiagnostics);
        }

        private async void RegisterDiagnostics(CompilationAnalysisContext context)
        {
            var smellReporter = new SmellReporter();
            var diagnostics = await smellReporter.Report(context.Compilation, SelectSmell());
            ReportDiagnostics(context, diagnostics);
        }

        protected abstract Smell SelectSmell();
    }
}
