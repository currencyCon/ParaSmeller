using System.Collections.Immutable;
using ConcurrencyAnalyzer.Diagnostics;
using ConcurrencyAnalyzer.Reporters.TentativelyResourceReferenceReporter;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ConcurrencyChecker.TentativelyResourceReference
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TentativelyResourceReferenceAnalyzer : BaseAnalyzer
    {
        private static readonly DiagnosticDescriptor TentativelyResourceReferenceRule = new DiagnosticDescriptor(TentativelyResourceReferenceReporter.DiagnosticId, TentativelyResourceReferenceReporter.Title, TentativelyResourceReferenceReporter.MessageFormatTentativelyResourceReference, TentativelyResourceReferenceReporter.Category, DiagnosticSeverity.Warning, true, TentativelyResourceReferenceReporter.Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(TentativelyResourceReferenceRule);

        protected override Smell SelectSmell()
        {
            return Smell.TenativelyRessource;
        }
    }
}
