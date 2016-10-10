
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConcurrencyAnalyzer.Representation
{
    public class InvocationExpressionRepresentation
    {
        public InvocationExpressionRepresentation(InvocationExpressionSyntax invocationExpressionSyntax)
        {
            MethodName = invocationExpressionSyntax.Expression.ToFullString();
        }

        public string MethodName { get; set; }
        public IMethodRepresentation MethodImplementation { get; set; }
        public bool Synchronized { get; set; }
        public ClassRepresentation ContainingClass { get; set; }
    }
}
