using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        private static readonly SemaphoreSlim SemaphoreSlim = new SemaphoreSlim(1);
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
                
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationAction(RegisterDiagnostics);
        }

        private async void RegisterDiagnostics(CompilationAnalysisContext context)
        {
            bool hasLock = await SemaphoreSlim.WaitAsync(new TimeSpan(0,0,0,1));
            if (hasLock)
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
                SemaphoreSlim.Release(1);
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
