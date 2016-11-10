
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace ConcurrencyAnalyzer.Representation
{
    public class LockBlock : Body
    {
        public override bool IsSynchronized => true;
        public LockBlock(Member member, SyntaxNode implementation)
        {
            Implementation = implementation;
            ContainingMember = member;
            InvocationExpressions = new List<InvocationExpressionRepresentation>();
            Blocks = new List<Body>();
        }
    }
}
