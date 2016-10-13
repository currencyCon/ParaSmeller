using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConcurrencyAnalyzer.Representation
{
    public class MethodRepresentation : IMethodRepresentation
    {
        public ICollection<InvocationExpressionRepresentation> InvocationExpressions { get; set; }
        public ClassRepresentation ContainingClass { get; set; }
        public ICollection<IBody> Blocks { get; set; }
        public SyntaxToken Name { get; set; }
        public MethodDeclarationSyntax MethodImplementation { get; set; }

        public MethodRepresentation(MethodDeclarationSyntax methodDeclarationSyntax, ClassRepresentation classRepresentation)
        {
            Name = methodDeclarationSyntax.Identifier;
            InvocationExpressions = new List<InvocationExpressionRepresentation>();
            MethodImplementation = methodDeclarationSyntax;
            Blocks = new List<IBody>();
            ContainingClass = classRepresentation;
        }
    }
}
