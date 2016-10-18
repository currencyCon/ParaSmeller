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
            var originalDefinition = "";
            if (methodInfo.Symbol != null)
            {
                type = methodInfo.Symbol.Kind;
                originalDefinition = GetOriginalDefinition(methodInfo);

            }
            var invocation = new InvocationExpressionRepresentation
            {
                Type = type,
                Implementation = invocationExpressionSyntax,
                ContainingBody = containingBody,
                Synchronized = containingBody?.Implementation.IsSynchronized() ?? false,
                InvocationTargetName = invocationTarget,
                CalledClass = className,
                OriginalDefinition = originalDefinition
            };
            return invocation;
            

        }

        private static string GetOriginalDefinition(SymbolInfo methodInfo)
        {
            var originalDefinition = "";
            if (methodInfo.Symbol is IMethodSymbol)
            {
                var methodSymbol = (IMethodSymbol) methodInfo.Symbol;
                originalDefinition = methodSymbol.ContainingType.OriginalDefinition + "." +
                                     methodSymbol.Name;
            } else if (methodInfo.Symbol is IPropertySymbol)
            {
                var propertySymbol = (IPropertySymbol) methodInfo.Symbol;
                originalDefinition = propertySymbol.OriginalDefinition.ToString();
            }
            return originalDefinition;
        }
    }
}
