
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace ConcurrencyAnalyzer.Representation
{
    public class NormalBlock : IBody
    {
        public NormalBlock(IMember member, SyntaxNode implementation)
        {
            ContainingMember = member;
            Implementation = implementation;
            InvocationExpressions = new List<IInvocationExpressionRepresentation>();
            Blocks = new List<IBody>();
        }

        public SyntaxNode Implementation { get; set; }
        public IMember ContainingMember { get; set; }
        public ICollection<IInvocationExpressionRepresentation> InvocationExpressions { get; set; }
        public ICollection<IBody> Blocks { get; set; }
        public bool IsSynchronized => false;
    }
}
