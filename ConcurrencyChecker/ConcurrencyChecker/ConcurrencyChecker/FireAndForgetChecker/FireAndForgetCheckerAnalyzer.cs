using System.Collections.Immutable;
using ConcurrencyAnalyzer.Diagnostics;
using ConcurrencyAnalyzer.Reporters;
using ConcurrencyAnalyzer.Reporters.FireAndForgetReporter;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Diagnostic = Microsoft.CodeAnalysis.Diagnostic;

namespace ConcurrencyChecker.FireAndForgetChecker
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class FireAndForgetCheckerAnalyzer: DiagnosticAnalyzer
    {

        private static readonly DiagnosticDescriptor RuleFireAndForgetCall = new DiagnosticDescriptor(FireAndForgetReporter.FireAndForgetCallId, FireAndForgetReporter.Title, FireAndForgetReporter.MessageFormatFireAndForghet, FireAndForgetReporter.Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: FireAndForgetReporter.Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(RuleFireAndForgetCall);
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationAction(CheckForUnawaitedTasks);
        }

        private static async void CheckForUnawaitedTasks(CompilationAnalysisContext context)
        {
            var smellReporter = new SmellReporter();
            var diagnostics = await smellReporter.Report(context.Compilation, Smell.FireAndForget);
            foreach (var diagnostic in diagnostics)
            {
                context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(diagnostic.Id, diagnostic.Title, diagnostic.MessageFormat, diagnostic.Category, DiagnosticSeverity.Warning, true, diagnostic.Description), diagnostic.Location));
            }
        }
    }
}
