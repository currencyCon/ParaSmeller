using System.Collections.Generic;
using System.Collections.Immutable;
using ConcurrencyAnalyzer.Diagnostics;
using ConcurrencyChecker.Analyzer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ConcurrencyChecker.Test.Finalizer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class FinalizerSynchronizationAnalyzer: BaseAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rules.FinalizerSynchronizationUsageRule);

        protected override ICollection<Smell> SelectSmell()
        {
            return new List<Smell> { Smell.Finalizer};
        }
    }
}
