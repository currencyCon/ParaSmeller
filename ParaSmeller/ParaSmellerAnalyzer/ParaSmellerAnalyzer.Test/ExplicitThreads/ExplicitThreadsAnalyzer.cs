using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using ParaSmellerAnalyzer.Analyzer;
using ParaSmellerCore.Diagnostics;

namespace ParaSmeller.Test.ExplicitThreads
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