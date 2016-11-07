using System.Collections.Generic;
using System.Collections.Immutable;
using ConcurrencyAnalyzer.Diagnostics;
using ConcurrencyChecker.Analyzer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ConcurrencyChecker.Test.HalfSynchronizedClass
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class HalfSynchronizedCheckerAnalyzer : BaseAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rules.HalfSynchronizedRule, Rules.UnsynchronizedPropertyRule);

        protected override ICollection<Smell> SelectSmell()
        {
            return new List<Smell> { Smell.HalfSynchronized};
        }
    }
}
