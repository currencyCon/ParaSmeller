
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace ConcurrencyAnalyzer.Representation
{
    public class LockBlock : IBody
    {
        public LockBlock(IMemberWithBody member, SyntaxNode implementation)
        {
            Implementation = implementation;
            ContainingMember = member;
            InvocationExpressions = new List<InvocationExpressionRepresentation>();
            Blocks = new List<IBody>();
        }

        public SyntaxNode Implementation { get; set; }
        public IMemberWithBody ContainingMember { get; set; }
        public ICollection<InvocationExpressionRepresentation> InvocationExpressions { get; set; }
        public ICollection<IBody> Blocks { get; set; }
    }
}
