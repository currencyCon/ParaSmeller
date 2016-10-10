
using System.Linq;
using ConcurrencyAnalyzer.Representation;
using ConcurrencyAnalyzer.SyntaxFilters;
using Microsoft.CodeAnalysis;
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

        public static IInvocationExpression Create(InvocationExpressionSyntax invocationExpressionSyntax, SemanticModel semanticModel)
        {
            var node = ((MemberAccessExpressionSyntax) invocationExpressionSyntax.Expression).Name;
            var methodInfo = semanticModel.GetSymbolInfo(node);
            var type = methodInfo.Symbol.Kind;
            var method = semanticModel.GetDeclaredSymbol(node);
            

            return null;
        }

        private static MethodInvocation CreateMethodInvocation(InvocationExpressionSyntax invocationExpressionSyntax)
        {
            return null;
        }
        private static PropertyInvocation CreatePropertyInvocation(InvocationExpressionSyntax invocationExpressionSyntax)
        {
            return null;
        }
    }
}
