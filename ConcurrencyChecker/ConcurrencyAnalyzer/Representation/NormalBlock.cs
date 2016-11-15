
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace ConcurrencyAnalyzer.Representation
{
    public class NormalBlock : Body
    {
        public override bool IsSynchronized => false;
        public NormalBlock(Member member, SyntaxNode implementation)
        {
            ContainingMember = member;
            Implementation = implementation;
            InvocationExpressions = new List<InvocationExpressionRepresentation>();
            Blocks = new List<Body>();
        }
        
    }
}
