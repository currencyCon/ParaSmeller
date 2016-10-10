using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace ConcurrencyAnalyzer.Representation
{
    public interface IBody
    {
        SyntaxNode Implementation { get; set; }
        IMemberWithBody ContainingMember { get; set; }
        ICollection<IInvocationExpression> InvocationExpressions { get; set; }
        ICollection<IBody> Blocks { get; set; }
    }
}
