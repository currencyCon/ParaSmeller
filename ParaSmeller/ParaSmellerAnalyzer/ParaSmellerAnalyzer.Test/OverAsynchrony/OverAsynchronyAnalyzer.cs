using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using ParaSmellerAnalyzer.Analyzer;
using ParaSmellerCore.Diagnostics;

namespace ParaSmeller.Test.OverAsynchrony
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