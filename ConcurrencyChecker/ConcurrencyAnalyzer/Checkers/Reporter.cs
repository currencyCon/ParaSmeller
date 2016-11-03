using System.Collections.Generic;
using ConcurrencyAnalyzer.Diagnostics;
using ConcurrencyAnalyzer.Representation;

namespace ConcurrencyAnalyzer.Checkers
{
    public interface IReporter
    {
        ICollection<Diagnostic> Report(SolutionRepresentation solutionRepresentation);
    }
}
