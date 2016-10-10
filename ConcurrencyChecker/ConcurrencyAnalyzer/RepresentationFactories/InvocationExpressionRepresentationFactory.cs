
using System.Linq;
using ConcurrencyAnalyzer.Representation;
using ConcurrencyAnalyzer.SyntaxFilters;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConcurrencyAnalyzer.RepresentationFactories
{
    public class InvocationExpressionRepresentationFactory
    {
        public static InvocationExpressionRepresentation Create(InvocationExpressionSyntax invocationExpressionSyntax, ClassRepresentation classRepresentation, IMethodRepresentation methodRepresentation)
        {
            var invocation = new InvocationExpressionRepresentation(invocationExpressionSyntax)
            {
                ContainingClass = classRepresentation
            };
            var synchronizedInvocations = SyntaxNodeFilter.GetSynchronizedInvocations(methodRepresentation.MethodImplementation);
            invocation.Synchronized = synchronizedInvocations.Contains(invocationExpressionSyntax);
            return invocation;
        }
    }
}
