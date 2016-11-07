using System.Collections.Generic;
using System.Collections.Immutable;
using ConcurrencyAnalyzer.Diagnostics;
using ConcurrencyChecker.Analyzer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ConcurrencyChecker.Test.OverAsynchrony
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class OverAsynchronyAnalyzer : BaseAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rules.PrivateAsyncRule, Rules.NestedAsyncRule);


        protected override ICollection<Smell> SelectSmell()
        {
            return new List<Smell> { Smell.OverAsynchrony};
        }
    }
}