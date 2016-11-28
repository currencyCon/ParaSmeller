using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using ParaSmellerAnalyzer.Analyzer;
using ParaSmellerCore.Diagnostics;

namespace ParaSmeller.Test.EasterEgg
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