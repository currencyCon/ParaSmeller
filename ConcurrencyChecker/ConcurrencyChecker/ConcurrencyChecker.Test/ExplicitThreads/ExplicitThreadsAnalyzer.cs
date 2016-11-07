using System.Collections.Generic;
using System.Collections.Immutable;
using ConcurrencyAnalyzer.Diagnostics;
using ConcurrencyChecker.Analyzer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ConcurrencyChecker.Test.ExplicitThreads
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ExplicitThreadsAnalyzer : BaseAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rules.ExplicitThreadsRule);

        protected override ICollection<Smell> SelectSmell()
        {
            return new List<Smell> { Smell.ExplicitThreads};
        }
    }
}