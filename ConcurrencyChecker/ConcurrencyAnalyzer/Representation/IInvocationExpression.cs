
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConcurrencyAnalyzer.Representation
{
    public interface IInvocationExpression
    {

        IdentifierNameSyntax CalledClass { get; set; }
        SimpleNameSyntax InvocationTargetName { get; set; }
        bool Synchronized { get; set; }
        IMemberWithBody InvocationImplementation { get; set; }
        InvocationExpressionSyntax Implementation { get; set; }
        IBody ContainingBody { get; set; }
        Microsoft.CodeAnalysis.SymbolKind Type { get; set; }
    }
}
