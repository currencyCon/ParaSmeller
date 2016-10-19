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

        public static IInvocationExpressionRepresentation Create(InvocationExpressionSyntax invocationExpressionSyntax, SemanticModel semanticModel, IBody containingBody = null)
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

        private static IInvocationExpressionRepresentation CreateSelfInvocation(
            InvocationExpressionSyntax invocationExpressionSyntax, SemanticModel semanticModel, IBody containingBody)
        {
            var invocationExpression = (IdentifierNameSyntax)invocationExpressionSyntax.Expression;
            var invocationTarget = invocationExpression;
            return CreateInvocationWithSymbolInfo(invocationExpressionSyntax, semanticModel, containingBody, invocationTarget);
        }



        private static IInvocationExpressionRepresentation CreateRemoteInvocation(
            InvocationExpressionSyntax invocationExpressionSyntax, SemanticModel semanticModel, IBody containingBody)
        {
            var invocationExpression = (MemberAccessExpressionSyntax) invocationExpressionSyntax.Expression;
            var invocationTarget = invocationExpression.Name;
            return CreateInvocationWithSymbolInfo(invocationExpressionSyntax, semanticModel, containingBody, invocationTarget);
        }

        private static IInvocationExpressionRepresentation CreateInvocation(
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

        private static IInvocationExpressionRepresentation CreateInvocationWithSymbolInfo(
            InvocationExpressionSyntax invocationExpressionSyntax, SemanticModel semanticModel, IBody containingBody,
            SimpleNameSyntax invocationTarget)
        {
            var symbolInfo = semanticModel.GetSymbolInfo(invocationTarget);
            var type = GetType(symbolInfo);
            var originalDefinition = GetOriginalDefinition(symbolInfo);
            var className = GetClassName(symbolInfo);
            return CreateInvocation(invocationExpressionSyntax, containingBody, type, invocationTarget, className,
                originalDefinition);
        }

        private static ISymbol GetSymbol(SymbolInfo symbolInfo)
        {
            if (symbolInfo.Symbol is IMethodSymbol)
            {
                return (IMethodSymbol)symbolInfo.Symbol;
            }
            if (symbolInfo.Symbol is IPropertySymbol)
            {
                return (IPropertySymbol)symbolInfo.Symbol;
            }
            return null;
        }

        private static SymbolKind GetType(SymbolInfo symbolInfo)
        {
            var type = SymbolKind.NetModule;
            var symbol = GetSymbol(symbolInfo);
            if (symbol != null)
            {
                type = symbolInfo.Symbol.Kind;
            }
            return type;
        }

        private static string GetClassName(SymbolInfo symbolInfo)
        {
            var className = "";
            var symbol = GetSymbol(symbolInfo);
            if (symbol != null)
            {
                className = symbol.ContainingType.Name;
            }
            return className;
        }
        private static string GetOriginalDefinition(SymbolInfo methodInfo)
        {
            var originalDefinition = "";
            var symbol = GetSymbol(methodInfo);
            if (symbol != null)
            {
                originalDefinition = symbol.ContainingType.OriginalDefinition + "." +
                                     symbol.Name;
            }
            return originalDefinition;
        }
    }
}
