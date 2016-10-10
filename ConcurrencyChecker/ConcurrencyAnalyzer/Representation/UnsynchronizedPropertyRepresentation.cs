
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConcurrencyAnalyzer.Representation
{
    public class UnsynchronizedPropertyRepresentation : IPropertyRepresentation
    {
        public PropertyDeclarationSyntax PropertyImplementation { get; set; }
        public ClassRepresentation ContainingClass { get; set; }
        public ICollection<IBody> Blocks { get; set; }
        public ICollection<InvocationExpressionRepresentation> InvocationExpressions { get; set; }
        public AccessorDeclarationSyntax Getter { get; set; }
        public AccessorDeclarationSyntax Setter { get; set; }

        public UnsynchronizedPropertyRepresentation(PropertyDeclarationSyntax propertyDeclarationSyntax, ClassRepresentation classRepresentation)
        {
            InvocationExpressions = new List<InvocationExpressionRepresentation>();
            PropertyImplementation = propertyDeclarationSyntax;
            ContainingClass = classRepresentation;
            Getter =
                propertyDeclarationSyntax.AccessorList.Accessors.FirstOrDefault(
                    e => e.Keyword.ToString() == SyntaxKind.GetKeyword.ToString());
            Setter =
                propertyDeclarationSyntax.AccessorList.Accessors.FirstOrDefault(
                    e => e.Keyword.ToString() == SyntaxKind.SetKeyword.ToString());
        }
    }
}
