
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConcurrencyAnalyzer.Representation
{
    public class InvocationExpressionRepresentation
    {
        public InvocationExpressionRepresentation(InvocationExpressionSyntax invocationExpressionSyntax)
        {
            var methodInvocation = (MemberAccessExpressionSyntax)invocationExpressionSyntax.Expression;
            CalledClass = (IdentifierNameSyntax)methodInvocation.Expression;
            MethodName = methodInvocation.Name;

        }
        public IdentifierNameSyntax CalledClass { get; set; }
        public SimpleNameSyntax MethodName { get; set; }
        public IMethodRepresentation MethodImplementation { get; set; }
        public bool Synchronized { get; set; }
        public ClassRepresentation ContainingClass { get; set; }
    }
}
