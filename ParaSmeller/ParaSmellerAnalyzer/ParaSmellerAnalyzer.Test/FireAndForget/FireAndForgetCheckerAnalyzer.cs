using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using ParaSmellerAnalyzer.Analyzer;
using ParaSmellerCore.Diagnostics;

namespace ParaSmeller.Test.FireAndForget
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class FireAndForgetCheckerAnalyzer: BaseAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rules.RuleFireAndForgetCallRule);
        protected override ICollection<Smell> SelectSmell()
        {
            return new List<Smell> { Smell.FireAndForget};
        }
    }
}
