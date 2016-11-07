using System.Collections.Immutable;
using ConcurrencyAnalyzer.Diagnostics;
using ConcurrencyAnalyzer.Reporters.TapirReporter;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ConcurrencyChecker.EasterEgg
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TapirrAnalyzer : BaseAnalyzer
    {
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(TapirReporter.DiagnosticId, TapirReporter.Title, TapirReporter.MessageFormat, TapirReporter.Category, DiagnosticSeverity.Warning, true, TapirReporter.Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);


        protected override Smell SelectSmell()
        {
            return Smell.Tapir;
        }

    }
}