using System.Collections.Generic;
using ConcurrencyAnalyzer.RepresentationFactories;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConcurrencyAnalyzer.Representation
{
    public class SynchronizedMethodRepresentation : IMethodRepresentation
    {
        
        public MethodDeclarationSyntax MethodImplementation { get; set; }
        public ClassRepresentation ContainingClass { get; set; }
        public ICollection<IBody> Blocks { get; set; }
        public ICollection<InvocationExpressionRepresentation> InvocationExpressions { get; set; }
        public SynchronizedMethodRepresentation(MethodDeclarationSyntax methodDeclarationSyntax, ClassRepresentation classRepresentation)
        {
            InvocationExpressions = new List<InvocationExpressionRepresentation>();
            MethodImplementation = methodDeclarationSyntax;
            Blocks = new List<IBody>();
            ContainingClass = classRepresentation;
        }
    }
}
