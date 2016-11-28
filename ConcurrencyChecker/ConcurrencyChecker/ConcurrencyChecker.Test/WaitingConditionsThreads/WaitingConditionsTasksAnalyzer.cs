using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using ParaSmellerAnalyzer.Analyzer;
using ParaSmellerCore.Diagnostics;

namespace ParaSmeller.Test.WaitingConditionsThreads
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
