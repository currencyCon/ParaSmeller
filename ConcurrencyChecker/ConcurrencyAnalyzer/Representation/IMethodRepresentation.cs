using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConcurrencyAnalyzer.Representation
{
    public interface IMethodRepresentation: IMemberWithBody
    {
        MethodDeclarationSyntax MethodImplementation { get; set; }
    }
}
