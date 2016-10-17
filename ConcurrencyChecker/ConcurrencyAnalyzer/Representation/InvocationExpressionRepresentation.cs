
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConcurrencyAnalyzer.Representation
{
    public class InvocationExpressionRepresentation: IInvocationExpression
    {
        public InvocationExpressionRepresentation(InvocationExpressionSyntax invocationExpressionSyntax)
        {
            var methodInvocation = (MemberAccessExpressionSyntax)invocationExpressionSyntax.Expression;
            CalledClass = (IdentifierNameSyntax)methodInvocation.Expression;
            InvocationTargetName = methodInvocation.Name;

        }

        public InvocationExpressionRepresentation()
        {
        }

        public IdentifierNameSyntax CalledClass { get; set; }
        public SimpleNameSyntax InvocationTargetName { get; set; }
        public bool Synchronized { get; set; }
        public InvocationExpressionSyntax Implementation { get; set; }
        public IBody ContainingBody { get; set; }
        public IMemberWithBody InvocationImplementation { get; set; }
        public SymbolKind Type { get; set; }
        public string OriginalDefinition { get; set; }

    }
}
