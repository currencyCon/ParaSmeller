using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConcurrencyAnalyzer.Representation
{
    public interface IMethodRepresentation: IMember
    {
        MethodDeclarationSyntax MethodImplementation { get; set; }
        ICollection<ParameterSyntax> Parameters { get; set; }
    }
}
