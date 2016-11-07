using System.Collections.Immutable;
using ConcurrencyAnalyzer.Diagnostics;
using ConcurrencyAnalyzer.Reporters.PrimitiveSynchronizationReporter;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ConcurrencyChecker.PrimitiveSynchronizationChecker
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class PrimitiveSynchronizationAnalyzer : BaseAnalyzer
    {

        private static readonly DiagnosticDescriptor PrimitiveSynchronizationUsageRule = new DiagnosticDescriptor(PrimitiveSynchronizationReporter.PrimitiveSynchronizationDiagnosticId, PrimitiveSynchronizationReporter.Title, PrimitiveSynchronizationReporter.MessageFormatPrimitiveSynchronization, PrimitiveSynchronizationReporter.Category, DiagnosticSeverity.Warning, true, PrimitiveSynchronizationReporter.Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(PrimitiveSynchronizationUsageRule);

        protected override Smell SelectSmell()
        {
            return Smell.PrimitiveSynchronization;
        }
    }
}
