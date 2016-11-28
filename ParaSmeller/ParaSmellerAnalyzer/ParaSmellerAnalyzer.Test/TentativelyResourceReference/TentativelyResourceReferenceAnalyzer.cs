using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using ParaSmellerAnalyzer.Analyzer;
using ParaSmellerCore.Diagnostics;

namespace ParaSmeller.Test.TentativelyResourceReference
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
