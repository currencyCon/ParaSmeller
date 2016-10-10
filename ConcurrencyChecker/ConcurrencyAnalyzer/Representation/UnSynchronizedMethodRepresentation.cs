using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConcurrencyAnalyzer.Representation
{
    public class UnSynchronizedMethodRepresentation : IMethodRepresentation
    {
        public MethodDeclarationSyntax MethodImplementation { get; set; }
        public ClassRepresentation ContainingClass { get; set; }
        public ICollection<InvocationExpressionRepresentation> InvocationExpressions { get; set; }
        public UnSynchronizedMethodRepresentation(MethodDeclarationSyntax methodDeclarationSyntax, ClassRepresentation classRepresentation)
        {
            MethodImplementation = methodDeclarationSyntax;
            InvocationExpressions = new List<InvocationExpressionRepresentation>();
            ContainingClass = classRepresentation;
        }
    }
}
