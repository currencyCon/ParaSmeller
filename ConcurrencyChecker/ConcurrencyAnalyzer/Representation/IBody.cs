using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace ConcurrencyAnalyzer.Representation
{
    public interface IBody
    {
        SyntaxNode Implementation { get; set; }
        IMember ContainingMember { get; set; }
        ICollection<InvocationExpressionRepresentation> InvocationExpressions { get; set; }
        ICollection<IBody> Blocks { get; set; }
        bool IsSynchronized { get; }

    }
}
