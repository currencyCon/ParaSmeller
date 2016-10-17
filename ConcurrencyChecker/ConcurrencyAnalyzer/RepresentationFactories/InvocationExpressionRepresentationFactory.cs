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

            var invocationExpression = (MemberAccessExpressionSyntax)invocationExpressionSyntax.Expression;
            var className = (IdentifierNameSyntax)invocationExpression.Expression;
            var invocationTarget = invocationExpression.Name;
            var methodInfo = semanticModel.GetSymbolInfo(invocationTarget);
            var type = SymbolKind.NetModule;
            if (methodInfo.Symbol != null)
            {
                type = methodInfo.Symbol.Kind;
            }
            string originalDefinition = GetOriginalDefinition(methodInfo);
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
            

        }

        private static string GetOriginalDefinition(SymbolInfo methodInfo)
        {
            var originalDefinition = "";
            var methodSymbol = methodInfo.Symbol as IMethodSymbol;
            if (methodSymbol != null)
            {
                originalDefinition = methodSymbol.OriginalDefinition.ToString();
            } else if (methodInfo.Symbol is IPropertySymbol)
            {
                
            }

            var propertySymbol = methodInfo.Symbol as IPropertySymbol;

            originalDefinition = propertySymbol.OriginalDefinition.ToString();

            return originalDefinition;
        }
    }
}
