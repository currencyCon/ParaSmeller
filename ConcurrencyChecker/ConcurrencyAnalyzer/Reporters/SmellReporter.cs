using System.Collections.Generic;
using System.Threading.Tasks;
using ConcurrencyAnalyzer.Diagnostics;
using ConcurrencyAnalyzer.RepresentationFactories;
using Microsoft.CodeAnalysis;
using Diagnostic = ConcurrencyAnalyzer.Diagnostics.Diagnostic;

namespace ConcurrencyAnalyzer.Reporters
{
    public class SmellReporter
    {
        private static readonly Dictionary<Smell, BaseReporter> Reporters = new Dictionary<Smell, BaseReporter>
        {
            {Smell.PrimitiveSynchronization, new PrimitiveSynchronizationReporter()},
            {Smell.FireAndForget, new FireAndForgetReporter() },
            {Smell.Finalizer, new FinalizerReporter() },
            {Smell.HalfSynchronized, new HalfSynchronizedReporter() },
            {Smell.MonitorWaitOrSignal, new MonitorOrWaitSignalReporter() },
            {Smell.ExplicitThreads, new ExplicitThreadsReporter()},
            {Smell.NestedSynchronization, new NestedSynchronizedMethodClassReporter() },
            {Smell.OverAsynchrony, new OverAsynchronyReporter() },
            {Smell.TenativelyRessource, new TentativelyResourceReferenceReporter() },
            {Smell.WaitingConditionsTasks, new WaitingConditionsTasksReporter() }
            {Smell.Tapir, new TapirReporter() }
        };

        public async Task<ICollection<Diagnostic>> Report(Compilation compilation)
        {
            var solutionModel = await SolutionRepresentationFactory.Create(compilation);
            var diagnostics = new List<Diagnostic>();
            foreach (var reporter in Reporters.Values)
            {
                diagnostics.AddRange(reporter.Report(solutionModel));
            }
            return diagnostics;
        }

        public async Task<ICollection<Diagnostic>> Report(Compilation compilation, ICollection<Smell> smells)
        {
            var solutionModel = await SolutionRepresentationFactory.Create(compilation);
            var diagnostics = new List<Diagnostic>();
            foreach (var smell in smells)
            {
                diagnostics.AddRange(Reporters[smell].Report(solutionModel));
            }
            return diagnostics;
        }

        public async Task<ICollection<Diagnostic>> Report(Compilation compilation, Smell smell)
        {
            return await Report(compilation, new List<Smell>{smell});
        }
    }
}
