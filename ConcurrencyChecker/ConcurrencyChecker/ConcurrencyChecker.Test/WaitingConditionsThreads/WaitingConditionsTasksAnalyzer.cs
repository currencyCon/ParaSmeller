using System.Collections.Generic;
using System.Collections.Immutable;
using ConcurrencyAnalyzer.Diagnostics;
using ConcurrencyChecker.Analyzer;
using Microsoft.CodeAnalysis;

namespace ConcurrencyChecker.Test.WaitingConditionsThreads
{
    public class WaitingConditionsTasksAnalyzer: BaseAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(Rules.WaitingConditionsTasksRule);

        protected override ICollection<Smell> SelectSmell()
        {
            return new List<Smell> { Smell.WaitingConditionsTasks };
        }
    }
}
