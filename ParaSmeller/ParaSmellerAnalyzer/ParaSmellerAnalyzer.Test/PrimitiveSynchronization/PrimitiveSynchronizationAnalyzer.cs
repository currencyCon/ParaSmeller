using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using ParaSmellerAnalyzer.Analyzer;
using ParaSmellerCore.Diagnostics;

namespace ParaSmeller.Test.PrimitiveSynchronization
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class PrimitiveSynchronizationAnalyzer : BaseAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rules.PrimitiveSynchronizationUsageRule);

        protected override ICollection<Smell> SelectSmell()
        {
            return new List<Smell> { Smell.PrimitiveSynchronization};
        }
    }
}
