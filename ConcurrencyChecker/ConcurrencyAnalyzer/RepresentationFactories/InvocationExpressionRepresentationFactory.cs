using ConcurrencyAnalyzer.Representation;
using ConcurrencyAnalyzer.SyntaxFilters;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConcurrencyAnalyzer.RepresentationFactories
{
    public class InvocationExpressionRepresentationFactory
    {

        public static InvocationExpressionRepresentation Create(InvocationExpressionSyntax invocationExpressionSyntax, SemanticModel semanticModel, IBody containingBody)
        {
            var invocationExpression = (MemberAccessExpressionSyntax) invocationExpressionSyntax.Expression;
            var className = (IdentifierNameSyntax) invocationExpression.Expression;
            var invocationTarget = invocationExpression.Name;
            var methodInfo = semanticModel.GetSymbolInfo(invocationTarget);

            var type = SymbolKind.NetModule;
            if (methodInfo.Symbol != null)
            {
                type = methodInfo.Symbol.Kind;
            }

            var invocation = new InvocationExpressionRepresentation
            {
                Type = type,
                Implementation = invocationExpressionSyntax,
                ContainingBody = containingBody,
                Synchronized = containingBody.Implementation.IsSynchronized(),
                InvocationTargetName = invocationTarget,
                CalledClass = className
            };

            return invocation;
        }
    }
}
