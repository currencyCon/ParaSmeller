using System.Collections.Generic;
using System.Collections.Immutable;
using ConcurrencyAnalyzer.Diagnostics;
using ConcurrencyChecker.Analyzer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ConcurrencyChecker.Test.FireAndForget
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
