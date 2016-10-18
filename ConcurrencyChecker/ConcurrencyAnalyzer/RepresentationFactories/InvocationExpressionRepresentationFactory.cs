using System;
using System.Linq;
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
            if (invocationExpressionSyntax.Expression is IdentifierNameSyntax)
            {
                return CreateSelfInvocation(invocationExpressionSyntax, semanticModel, containingBody);

            }
            if (invocationExpressionSyntax.Expression is MemberAccessExpressionSyntax)
            {
                return CreateRemoteInvocation(invocationExpressionSyntax, semanticModel, containingBody);
            }
            throw new NotImplementedException($"An unexpected Type of invocationExpression was encountered: {invocationExpressionSyntax.ToFullString()}");
        }

        private static InvocationExpressionRepresentation CreateSelfInvocation(
            InvocationExpressionSyntax invocationExpressionSyntax, SemanticModel semanticModel, IBody containingBody)
        {
            var invocationExpression = (IdentifierNameSyntax)invocationExpressionSyntax.Expression;
            var className = invocationExpressionSyntax.GetFirstParent<ClassDeclarationSyntax>().Identifier.ToString();
            var invocationTarget = invocationExpression;
            return CreateInvocationWithSymbolInfo(invocationExpressionSyntax, semanticModel, containingBody, invocationTarget, className);
        }



        private static InvocationExpressionRepresentation CreateRemoteInvocation(
            InvocationExpressionSyntax invocationExpressionSyntax, SemanticModel semanticModel, IBody containingBody)
        {
            var invocationExpression = (MemberAccessExpressionSyntax) invocationExpressionSyntax.Expression;
            var className = ((IdentifierNameSyntax) invocationExpression.Expression).ToString();
            var invocationTarget = invocationExpression.Name;
            return CreateInvocationWithSymbolInfo(invocationExpressionSyntax, semanticModel, containingBody, invocationTarget, className);
        }

        private static InvocationExpressionRepresentation CreateInvocation(
            InvocationExpressionSyntax invocationExpressionSyntax, IBody containingBody, SymbolKind type,
            SimpleNameSyntax invocationTarget, string className, string originalDefinition)
        {
            var invocation = new InvocationExpressionRepresentation
            {
                Type = type,
                Arguments = invocationExpressionSyntax.ArgumentList.Arguments.SelectMany(e => e.GetChildren<IdentifierNameSyntax>()).ToList(),
                Implementation = invocationExpressionSyntax,
                ContainingBody = containingBody,
                Synchronized = containingBody?.Implementation.IsSynchronized() ?? false,
                InvocationTargetName = invocationTarget,
                CalledClass = className,
                OriginalDefinition = originalDefinition
            };
            return invocation;
        }

        private static InvocationExpressionRepresentation CreateInvocationWithSymbolInfo(
            InvocationExpressionSyntax invocationExpressionSyntax, SemanticModel semanticModel, IBody containingBody,
            SimpleNameSyntax invocationTarget, string className)
        {
            var methodInfo = semanticModel.GetSymbolInfo(invocationTarget);
            var type = SymbolKind.NetModule;
            var originalDefinition = "";
            if (methodInfo.Symbol != null)
            {
                type = methodInfo.Symbol.Kind;
                originalDefinition = GetOriginalDefinition(methodInfo);
            }
            return CreateInvocation(invocationExpressionSyntax, containingBody, type, invocationTarget, className,
                originalDefinition);
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
