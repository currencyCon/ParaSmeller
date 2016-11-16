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
        private static readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1);
        protected static void ReportDiagnostics(CompilationAnalysisContext context, ICollection<ConcurrencyAnalyzer.Diagnostics.Diagnostic> diagnostics)
        {
            Logger.DebugLog($"Found {diagnostics.Count} diagnostics");
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
            bool hasLock = await _semaphoreSlim.WaitAsync(new TimeSpan(0,0,0,1));
            if (hasLock)
            {
                Logger.DebugLog("Starting Analyzer");
                if (SelectSmell().Any())
                {
                    var smellReporter = new SmellReporter();
                    ICollection<ConcurrencyAnalyzer.Diagnostics.Diagnostic> diagnostics;
                    diagnostics = await smellReporter.Report(context.Compilation, SelectSmell());
                    ReportDiagnostics(context, diagnostics);

                }
                else
                {
                    var smellReporter = new SmellReporter();
                    ICollection<ConcurrencyAnalyzer.Diagnostics.Diagnostic> diagnostics;
                    diagnostics = await smellReporter.Report(context.Compilation);
                    ReportDiagnostics(context, diagnostics);
                }

                _semaphoreSlim.Release(1);
            }
        }

        protected virtual ICollection<Smell> SelectSmell()
        {
            return new List<Smell>();
        }
    }
}
