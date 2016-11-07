using System.Collections.Generic;
using System.Collections.Immutable;
using ConcurrencyAnalyzer.Diagnostics;
using ConcurrencyChecker.Analyzer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ConcurrencyChecker.Test.TentativelyResourceReference
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TentativelyResourceReferenceAnalyzer : BaseAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rules.TentativelyResourceReferenceRule);

        protected override ICollection<Smell> SelectSmell()
        {
            return new List<Smell>{ Smell.TenativelyRessource };
        }
    }
}
