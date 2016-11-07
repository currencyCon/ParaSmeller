using System.Collections.Immutable;
using ConcurrencyAnalyzer.Diagnostics;
using ConcurrencyAnalyzer.Reporters.FinalizerReporter;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ConcurrencyChecker.FinalizerChecker
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class FinalizerSynchronizationAnalyzer: BaseAnalyzer
    {

        private static readonly DiagnosticDescriptor FinalizerSynchronizationUsageRule = new DiagnosticDescriptor(FinalizerReporter.FinalizerSynchronizationDiagnosticId, FinalizerReporter.Title, FinalizerReporter.MessageFormatFinalizerSynchronization, FinalizerReporter.Category, DiagnosticSeverity.Warning, true, FinalizerReporter.Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(FinalizerSynchronizationUsageRule);

        protected override Smell SelectSmell()
        {
            return Smell.Finalizer;
        }
    }
}
