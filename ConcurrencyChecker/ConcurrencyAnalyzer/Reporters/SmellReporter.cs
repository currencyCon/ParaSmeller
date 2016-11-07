﻿using System.Collections.Generic;
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
            {Smell.PrimitiveSynchronization, new PrimitiveSynchronizationReporter.PrimitiveSynchronizationReporter()},
            {Smell.FireAndForget, new FireAndForgetReporter.FireAndForgetReporter() },
            {Smell.Finalizer, new FinalizerReporter.FinalizerReporter() },
            {Smell.HalfSynchronized, new HalfSynchronizedReporter.HalfSynchronizedReporter() },
            {Smell.MonitorWaitOrSignal, new MonitorOrWaitSignalReporter.MonitorOrWaitSignalReporter() },
            {Smell.ExplicitThreads, new ExplicitThreadsReporter.ExplicitThreadsReporter()},
            {Smell.NestedSynchronization, new NestedSynchronizedMethodClassReporter.NestedSynchronizedMethodClassReporter() },
            {Smell.OverAsynchrony, new OverAsynchronyReporter.OverAsynchronyReporter() },
            {Smell.TenativelyRessource, new TentativelyResourceReferenceReporter.TentativelyResourceReferenceReporter() }
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

        public async Task<ICollection<Diagnostic>> Report(Compilation compilation, Smell smell)
        {
            var solutionModel = await SolutionRepresentationFactory.Create(compilation);
            return Reporters[smell].Report(solutionModel);
        }
    }
}