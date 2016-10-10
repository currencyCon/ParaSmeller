using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConcurrencyAnalyzer.Representation
{
    public interface IMethodRepresentation
    {
        MethodDeclarationSyntax MethodImplementation { get; set; }
        ClassRepresentation ContainingClass { get; set; }
        ICollection<InvocationExpressionRepresentation> InvocationExpressions { get; set; }
    }
}
