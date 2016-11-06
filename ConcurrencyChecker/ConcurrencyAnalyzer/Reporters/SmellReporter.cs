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
        private static readonly Dictionary<Smell, IReporter> Reporters = new Dictionary<Smell, IReporter>
        {
            {Smell.PrimitiveSynchronization, new PrimitiveSynchronizationReporter.PrimitiveSynchronizationReporter()},
            {Smell.FireAndForget, new FireAndForgetReporter.FireAndForgetReporter() }
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
