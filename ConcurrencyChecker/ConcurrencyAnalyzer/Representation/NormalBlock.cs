
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace ConcurrencyAnalyzer.Representation
{
    public class NormalBlock : IBody
    {
        public SyntaxNode Implementation { get; set; }
        public IMember ContainingMember { get; set; }
        public ICollection<InvocationExpressionRepresentation> InvocationExpressions { get; set; }
        public ICollection<IBody> Blocks { get; set; }
        public bool IsSynchronized => false;

        public NormalBlock(IMember member, SyntaxNode implementation)
        {
            ContainingMember = member;
            Implementation = implementation;
            InvocationExpressions = new List<InvocationExpressionRepresentation>();
            Blocks = new List<IBody>();
        }
    }
}
