
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace ConcurrencyAnalyzer.Representation
{
    public class NormalBlock : IBody
    {
        public NormalBlock(IMemberWithBody member, SyntaxNode implementation)
        {
            ContainingMember = member;
            Implementation = implementation;
            InvocationExpressions = new List<IInvocationExpression>();
            Blocks = new List<IBody>();
        }

        public SyntaxNode Implementation { get; set; }
        public IMemberWithBody ContainingMember { get; set; }
        public ICollection<IInvocationExpression> InvocationExpressions { get; set; }
        public ICollection<IBody> Blocks { get; set; }
    }
}
