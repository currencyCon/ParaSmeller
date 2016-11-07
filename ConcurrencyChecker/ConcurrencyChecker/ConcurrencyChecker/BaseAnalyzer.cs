using System.Threading.Tasks;
using ConcurrencyAnalyzer.Diagnostics;
using ConcurrencyAnalyzer.Reporters;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Diagnostic = Microsoft.CodeAnalysis.Diagnostic;

namespace ConcurrencyChecker
{
    public abstract class BaseAnalyzer: DiagnosticAnalyzer
    {
        protected static async Task ReportDiagnostics(CompilationAnalysisContext context, Smell smell)
        {
            var smellReporter = new SmellReporter();
            var diagnostics = await smellReporter.Report(context.Compilation, smell);
            foreach (var diagnostic in diagnostics)
            {
                context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(diagnostic.Id, diagnostic.Title, diagnostic.MessageFormat, diagnostic.Category, DiagnosticSeverity.Warning, true, diagnostic.Description), diagnostic.Location));
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationAction(RegisterDiagnostics);
        }

        private async void RegisterDiagnostics(CompilationAnalysisContext context)
        {
            await ReportDiagnostics(context, SelectSmell());
        }

        protected abstract Smell SelectSmell();
    }
}
