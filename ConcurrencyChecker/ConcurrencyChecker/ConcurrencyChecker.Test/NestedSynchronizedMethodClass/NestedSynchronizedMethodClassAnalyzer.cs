using System.Collections.Generic;
using System.Collections.Immutable;
using ConcurrencyAnalyzer.Diagnostics;
using ConcurrencyChecker.Analyzer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ConcurrencyChecker.Test.NestedSynchronizedMethodClass
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class NestedSynchronizedMethodClassAnalyzer : BaseAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rules.NestedLockingOneRule, Rules.NestedLockingTwoRule);


        protected override ICollection<Smell> SelectSmell()
        {
            return new List<Smell> { Smell.NestedSynchronization};
        }
    }
}
