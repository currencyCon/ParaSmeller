using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace ConcurrencyAnalyzer.Representation
{
    public abstract class Member
    {
        public ICollection<InvocationExpressionRepresentation> InvocationExpressions { get; set; }
        public ClassRepresentation ContainingClass { get; set; }
        public InterfaceRepresentation ContainingInterface { get; set; }
        public ICollection<Body> Blocks { get; set; }
        public SyntaxToken Name { get; set; }
        public abstract bool IsFullySynchronized();
        public ICollection<InvocationExpressionRepresentation> Callers { get; set; }
        public string OriginalDefinition { get; set; }

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
