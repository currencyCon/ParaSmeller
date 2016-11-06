using System.Collections.Immutable;
using ConcurrencyAnalyzer.Diagnostics;
using ConcurrencyAnalyzer.Reporters;
using ConcurrencyAnalyzer.Reporters.HalfSynchronizedReporter;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Diagnostic = Microsoft.CodeAnalysis.Diagnostic;

namespace ConcurrencyChecker.HalfSynchronizedChecker
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class HalfSynchronizedCheckerAnalyzer : DiagnosticAnalyzer
    {

        private static readonly DiagnosticDescriptor RuleHalfSynchronized = new DiagnosticDescriptor(HalfSynchronizedReporter.HalfSynchronizedChildDiagnosticId, HalfSynchronizedReporter.Title, HalfSynchronizedReporter.MessageFormatHalfSynchronized, HalfSynchronizedReporter.Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: HalfSynchronizedReporter.Description);
        private static readonly DiagnosticDescriptor RuleUnsynchronizedProperty = new DiagnosticDescriptor(HalfSynchronizedReporter.UnsynchronizedPropertyId, HalfSynchronizedReporter.Title, HalfSynchronizedReporter.MessageFormatUnsychronizedProperty, HalfSynchronizedReporter.Category, DiagnosticSeverity.Warning, isEnabledByDefault:true, description: HalfSynchronizedReporter.Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(RuleHalfSynchronized, RuleUnsynchronizedProperty);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationAction(AnalyzeHalfSynchronizedClass);

        }

        private static async void AnalyzeHalfSynchronizedClass(CompilationAnalysisContext context)
        {
            var smellReporter = new SmellReporter();
            var diagnostics = await smellReporter.Report(context.Compilation, Smell.HalfSynchronized);
            foreach (var diagnostic in diagnostics)
            {
                context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(diagnostic.Id, diagnostic.Title, diagnostic.MessageFormat, diagnostic.Category, DiagnosticSeverity.Warning, true, diagnostic.Description), diagnostic.Location, diagnostic.Params));
            }
        }
    }
}
