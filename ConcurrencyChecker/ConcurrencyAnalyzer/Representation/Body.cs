using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace ConcurrencyAnalyzer.Representation
{
    public abstract class Body
    {
        public SyntaxNode Implementation { get; set; }
        public Member ContainingMember { get; set; }
        public ICollection<InvocationExpressionRepresentation> InvocationExpressions { get; set; }
        public ICollection<Body> Blocks { get; set; }
        public abstract bool IsSynchronized { get; }
        protected Body(Member member, SyntaxNode implementation)
        {
            Implementation = implementation;
            ContainingMember = member;
            InvocationExpressions = new List<InvocationExpressionRepresentation>();
            Blocks = new List<Body>();
        }
        public ICollection<InvocationExpressionRepresentation> GetAllInvocations()
        {
            var invocations = InvocationExpressions.ToList();
            foreach (var block in Blocks)
            {
                invocations.AddRange(block.GetAllInvocations());
            }
            return invocations;
        }
    }
}
