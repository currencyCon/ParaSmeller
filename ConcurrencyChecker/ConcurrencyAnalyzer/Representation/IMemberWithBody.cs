using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Microsoft.CodeAnalysis;

namespace ConcurrencyAnalyzer.Representation
{
    public interface IMemberWithBody
    {
        ICollection<IInvocationExpressionRepresentation> InvocationExpressions { get; set; }
        ClassRepresentation ContainingClass { get; set; }
        ICollection<IBody> Blocks { get; set; }
        SyntaxToken Name { get; set; }
        bool IsFullySynchronized();
        ICollection<IInvocationExpressionRepresentation> Callers { get; set; }

        
    }
}
