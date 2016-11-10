using System.Collections.Generic;
using System.Collections.Immutable;
using ConcurrencyAnalyzer.Diagnostics;
using ConcurrencyChecker.Analyzer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ConcurrencyChecker.Test.EasterEgg
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TapirrAnalyzer : BaseAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rules.TapirRule);
        
        protected override ICollection<Smell> SelectSmell()
        {
            return new List<Smell> { Smell.Tapir};
        }

    }
}