using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace ConcurrencyAnalyzer.Representation
{
    public interface IMemberWithBody
    {
        ICollection<InvocationExpressionRepresentation> InvocationExpressions { get; set; }
        ClassRepresentation ContainingClass { get; set; }
        ICollection<IBody> Blocks { get; set; }
        SyntaxToken Name { get; set; }
        bool IsFullySynchronized();
    }
}
