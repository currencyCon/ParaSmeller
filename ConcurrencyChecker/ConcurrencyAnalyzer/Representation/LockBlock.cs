
using System.Collections.Generic;
using System.Linq;
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
