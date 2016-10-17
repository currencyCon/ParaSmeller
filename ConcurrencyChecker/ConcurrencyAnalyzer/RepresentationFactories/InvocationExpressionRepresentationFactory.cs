using ConcurrencyAnalyzer.Representation;
using ConcurrencyAnalyzer.SyntaxFilters;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConcurrencyAnalyzer.RepresentationFactories
{
    public class InvocationExpressionRepresentationFactory
    {

        public static InvocationExpressionRepresentation Create(InvocationExpressionSyntax invocationExpressionSyntax, SemanticModel semanticModel, IBody containingBody = null)
        {
            if (invocationExpressionSyntax.Expression is MemberAccessExpressionSyntax)
            {
                var invocationExpression = (MemberAccessExpressionSyntax)invocationExpressionSyntax.Expression;
                var className = (IdentifierNameSyntax)invocationExpression.Expression;
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
                    Synchronized = containingBody?.Implementation.IsSynchronized() ?? false,
                    InvocationTargetName = invocationTarget,
                    CalledClass = className
                };

                return invocation;
            } else if (invocationExpressionSyntax.Parent is ParenthesizedLambdaExpressionSyntax)
            {
                //var expression = invocationExpressionSyntax a
                //var c = 2;
                return null;
            }
            return null;

        }
    }
}
