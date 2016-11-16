using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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

        public void AppendLockArguments(ICollection<string> lockObjects)
        {
            if (IsSynchronized)
            {
                lockObjects.Add(((LockStatementSyntax) Implementation).Expression.ToString());
            }
            foreach (var subLockBlock in Blocks)
            {
                subLockBlock.AppendLockArguments(lockObjects);
            }
        }
    }
}
