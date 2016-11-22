using System.Collections.Generic;
using System.Linq;
using ConcurrencyAnalyzer.SyntaxNodeUtils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConcurrencyAnalyzer.Representation
{
    public abstract class Member
    {
        public readonly ICollection<Body> Blocks;
        public readonly SyntaxToken Name;
        public ClassRepresentation ContainingClass { get; set; }
        public InterfaceRepresentation ContainingInterface { get; set; }
        public abstract bool IsFullySynchronized();
        public readonly string OriginalDefinition;
        
        protected Member(string originalDefinition, SyntaxToken name)
        {
            Blocks = new List<Body>();
            OriginalDefinition = originalDefinition;
            Name = name;
        }
        public ICollection<InvocationExpressionRepresentation> GetAllInvocations()
        {
            var invocations = new List<InvocationExpressionRepresentation>();
            foreach (var block in Blocks)
            {
                invocations.AddRange(block.GetAllInvocations());
            }
            return invocations;
        }

        public IEnumerable<TChildren> GetChildren<TChildren>()
        {
            return Blocks.SelectMany(e => e.Implementation.GetChildren<TChildren>());
        }

        public List<string> GetAllLockArguments()
        {
            var lockObjects = new List<string>();

            foreach (var block in Blocks)
            {
                if (block is LockBlock)
                {
                    lockObjects.Add(((LockStatementSyntax)block.Implementation).Expression.ToString());
                }
                block.AppendLockArguments(lockObjects);
            }
            
            return lockObjects;
        }
    }
}
