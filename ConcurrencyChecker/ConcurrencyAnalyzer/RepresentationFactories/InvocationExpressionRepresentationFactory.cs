using System;
using System.Linq;
using ConcurrencyAnalyzer.Representation;
using ConcurrencyAnalyzer.SemanticAnalysis;
using ConcurrencyAnalyzer.SyntaxFilters;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConcurrencyAnalyzer.RepresentationFactories
{
    public static class InvocationExpressionRepresentationFactory
    {

        public static InvocationExpressionRepresentation Create(InvocationExpressionSyntax invocationExpressionSyntax, SemanticModel semanticModel, IBody containingBody)
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
            var invocationTarget = invocationExpression;
            return CreateInvocationWithSymbolInfo(invocationExpressionSyntax, semanticModel, containingBody, invocationTarget);
        }



        private static InvocationExpressionRepresentation CreateRemoteInvocation(
            InvocationExpressionSyntax invocationExpressionSyntax, SemanticModel semanticModel, IBody containingBody)
        {
            var invocationExpression = (MemberAccessExpressionSyntax) invocationExpressionSyntax.Expression;
            var invocationTarget = invocationExpression.Name;
            return CreateInvocationWithSymbolInfo(invocationExpressionSyntax, semanticModel, containingBody, invocationTarget);
        }

        private static InvocationExpressionRepresentation CreateInvocationWithSymbolInfo(
    InvocationExpressionSyntax invocationExpressionSyntax, SemanticModel semanticModel, IBody containingBody,
    SimpleNameSyntax invocationTarget)
        {
            var symbolInfo = SymbolInformationBuilder.Create(invocationTarget, semanticModel);
            return CreateInvocation(invocationExpressionSyntax, containingBody, invocationTarget, symbolInfo);
        }

        private static InvocationExpressionRepresentation CreateInvocation(
            InvocationExpressionSyntax invocationExpressionSyntax, IBody containingBody, SimpleNameSyntax invocationTarget, SymbolInformation symbolInfo)
        {
            var invocationIsSynchronized = containingBody?.Implementation.IsSynchronized() ?? false;
            var invocation = new InvocationExpressionRepresentation(invocationIsSynchronized, symbolInfo, invocationExpressionSyntax, containingBody, invocationTarget);
            var arguments =
                invocationExpressionSyntax.ArgumentList.Arguments.SelectMany(e => e.GetChildren<IdentifierNameSyntax>())
                    .ToList();
            invocation.Arguments.AddRange(arguments);
            return invocation;
        }
    }
}
