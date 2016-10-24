using System;
using System.Linq;
using ConcurrencyAnalyzer.Representation;
using ConcurrencyAnalyzer.SemanticAnalysis;
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
            InvocationExpressionSyntax invocationExpressionSyntax, IBody containingBody, SimpleNameSyntax invocationTarget, SymbolInformation symbolInfo)
        {
            var invocation = new InvocationExpressionRepresentation
            {
                Type = symbolInfo.Type,
                Arguments = invocationExpressionSyntax.ArgumentList.Arguments.SelectMany(e => e.GetChildren<IdentifierNameSyntax>()).ToList(),
                Implementation = invocationExpressionSyntax,
                ContainingBody = containingBody,
                Synchronized = containingBody?.Implementation.IsSynchronized() ?? false,
                InvocationTargetName = invocationTarget,
                CalledClass = symbolInfo.ClassName,
                OriginalDefinition = symbolInfo.OriginalDefinition
            };
            return invocation;
        }

        private static IInvocationExpressionRepresentation CreateInvocationWithSymbolInfo(
            InvocationExpressionSyntax invocationExpressionSyntax, SemanticModel semanticModel, IBody containingBody,
            SimpleNameSyntax invocationTarget)
        {
            var symbolInfo = SymbolInformationBuilder.Create(invocationTarget, semanticModel);
            return CreateInvocation(invocationExpressionSyntax, containingBody, invocationTarget, symbolInfo);
        }
    }
}
