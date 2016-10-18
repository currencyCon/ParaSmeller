
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConcurrencyAnalyzer.Representation
{
    public interface IInvocationExpression
    {

        string CalledClass { get; set; }
        SimpleNameSyntax InvocationTargetName { get; set; }
        bool Synchronized { get; set; }
        IMemberWithBody InvocationImplementation { get; set; }
        InvocationExpressionSyntax Implementation { get; set; }
        IBody ContainingBody { get; set; }
        Microsoft.CodeAnalysis.SymbolKind Type { get; set; }
        ICollection<IdentifierNameSyntax> Arguments { get; set; }
    }
}
