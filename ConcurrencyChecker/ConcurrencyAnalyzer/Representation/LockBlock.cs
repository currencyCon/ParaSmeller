
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace ConcurrencyAnalyzer.Representation
{
    public class LockBlock : IBody
    {
        public SyntaxNode Implementation { get; set; }
        public IMember ContainingMember { get; set; }
        public ICollection<InvocationExpressionRepresentation> InvocationExpressions { get; set; }
        public ICollection<IBody> Blocks { get; set; }
        public bool IsSynchronized => true;

        public LockBlock(IMember member, SyntaxNode implementation)
        {
            Implementation = implementation;
            ContainingMember = member;
            InvocationExpressions = new List<InvocationExpressionRepresentation>();
            Blocks = new List<IBody>();
        }
    }
}
