
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ConcurrencyChecker.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AllAnalyzer : BaseAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
            Rules.FinalizerSynchronizationUsageRule, 
            Rules.ExplicitThreadsRule, 
            Rules.HalfSynchronizedRule, 
            Rules.MonitorIfRule, 
            Rules.MonitorPulseRule, 
            Rules.NestedAsyncRule, 
            Rules.NestedLockingOneRule, 
            Rules.NestedLockingTwoRule, 
            Rules.PrimitiveSynchronizationUsageRule, 
            Rules.PrivateAsyncRule, 
            Rules.RuleFireAndForgetCallRule, 
            Rules.TapirRule, 
            Rules.TentativelyResourceReferenceRule, 
            Rules.UnsynchronizedPropertyRule, 
            Rules.WaitingConditionsTasksRule);
    }
}
