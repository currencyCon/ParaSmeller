using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConcurrencyAnalyzer.Representation
{
    public class PropertyRepresentation : IMember
    {
        private const string GetKeyWord = "get";
        private const string SetKeyWord = "set";

        public ICollection<InvocationExpressionRepresentation> InvocationExpressions { get; set; }
        public ClassRepresentation ContainingClass { get; set; }
        public ICollection<IBody> Blocks { get; set; }
        public SyntaxToken Name { get; set; }
        public ICollection<InvocationExpressionRepresentation> Callers { get; set; }
        public PropertyDeclarationSyntax PropertyImplementation { get; set; }
        public BlockSyntax Getter { get; set; }
        public BlockSyntax Setter { get; set; }


        public PropertyRepresentation(PropertyDeclarationSyntax propertyDeclarationSyntax, ClassRepresentation classRepresentation)
        {
            InvocationExpressions = new List<InvocationExpressionRepresentation>();
            Blocks = new List<IBody>();
            PropertyImplementation = propertyDeclarationSyntax;
            Name = PropertyImplementation.Identifier;
            ContainingClass = classRepresentation;
            Getter =
                propertyDeclarationSyntax.AccessorList.Accessors.FirstOrDefault(
                    e => e.Keyword.ToString() == GetKeyWord)?.Body;
            Setter =
            propertyDeclarationSyntax.AccessorList.Accessors.FirstOrDefault(
                e => e.Keyword.ToString() == SetKeyWord)?.Body;
            Callers = new List<InvocationExpressionRepresentation>();
        }

        public bool IsFullySynchronized()
        {
            return AllAccessorsAreSynchronized();
        }

        private bool AllAccessorsAreSynchronized()
        {
            if (Blocks.Count != 2)
            {
                return false;
            }
            var isFullySynchronized = true;
            foreach (var block in Blocks)
            {
                if (!(block.Blocks.Count == 1 && block.Blocks.First().IsSynchronized))
                {
                    isFullySynchronized = false;
                }
            }
            return isFullySynchronized;
        }
    }
}
