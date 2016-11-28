using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ParaSmellerCore.Representation;
using ParaSmellerCore.SemanticAnalysis;
using ParaSmellerCore.SyntaxNodeUtils;

namespace ParaSmellerCore.RepresentationFactories
{
    public static class InvocationExpressionRepresentationFactory
    {

        public static InvocationExpressionRepresentation Create(InvocationExpressionSyntax invocationExpressionSyntax, SemanticModel semanticModel, Body containingBody)
        {
            if (invocationExpressionSyntax.Expression is IdentifierNameSyntax)
            {
                return CreateSelfInvocation(invocationExpressionSyntax, semanticModel, containingBody);
            }
            if (invocationExpressionSyntax.Expression is MemberAccessExpressionSyntax)
            {
                return CreateRemoteInvocation(invocationExpressionSyntax, semanticModel, containingBody);
            }
            if (invocationExpressionSyntax.Expression is GenericNameSyntax)
            {
                return CreateGenericInvocation(invocationExpressionSyntax, semanticModel, containingBody);
            }
            if (invocationExpressionSyntax.Expression is ElementAccessExpressionSyntax)
            {
                return CreateElementAccessInvocation(invocationExpressionSyntax, semanticModel, containingBody);
            }
            if (invocationExpressionSyntax.Expression is ParenthesizedExpressionSyntax)
            {
                return CreateParenthesizedInvocation(invocationExpressionSyntax, semanticModel, containingBody);
            }
            if (invocationExpressionSyntax.Expression is InvocationExpressionSyntax)
            {
                return Create((InvocationExpressionSyntax)invocationExpressionSyntax.Expression, semanticModel, containingBody);
            }
            if (invocationExpressionSyntax.Expression is MemberBindingExpressionSyntax)
            {
                if (invocationExpressionSyntax.Parent is ConditionalAccessExpressionSyntax)
                {
                    return CreateBindingInvocation(invocationExpressionSyntax, semanticModel, containingBody);
                }
                var invocation = invocationExpressionSyntax.ArgumentList.GetFirstChild<InvocationExpressionSyntax>();
                return CreateSelfInvocation(invocation, semanticModel, containingBody);
            }
            throw new NotImplementedException($"An unexpected Type of invocationExpression was encountered: {invocationExpressionSyntax.ToFullString()}");
        }
        
        private static InvocationExpressionRepresentation CreateParenthesizedInvocation(InvocationExpressionSyntax invocationExpressionSyntax, SemanticModel semanticModel, Body containingBody)
        {
            var invocationExpression = (ParenthesizedExpressionSyntax)invocationExpressionSyntax.Expression;
            var invocationTarget = invocationExpression;
            return CreateInvocationWithSymbolInfo(invocationExpressionSyntax, semanticModel, containingBody, invocationTarget.GetFirstChild<SimpleNameSyntax>());
        }

        private static InvocationExpressionRepresentation CreateElementAccessInvocation(InvocationExpressionSyntax invocationExpressionSyntax, SemanticModel semanticModel, Body containingBody)
        {
            var invocationExpression = (ElementAccessExpressionSyntax) invocationExpressionSyntax.Expression;
            var invocationTarget = invocationExpression;
            return CreateInvocationWithSymbolInfo(invocationExpressionSyntax, semanticModel, containingBody, invocationTarget.GetFirstChild<SimpleNameSyntax>());
        }

        private static InvocationExpressionRepresentation CreateGenericInvocation(InvocationExpressionSyntax invocationExpressionSyntax, SemanticModel semanticModel, Body containingBody)
        {
            var invocationExpression = (GenericNameSyntax) invocationExpressionSyntax.Expression;
            var invocationTarget = invocationExpression;
            return CreateInvocationWithSymbolInfo(invocationExpressionSyntax, semanticModel, containingBody, invocationTarget);
        }

        private static InvocationExpressionRepresentation CreateSelfInvocation(
            InvocationExpressionSyntax invocationExpressionSyntax, SemanticModel semanticModel, Body containingBody)
        {
            var invocationExpression = (IdentifierNameSyntax)invocationExpressionSyntax.Expression;
            var invocationTarget = invocationExpression;
            var invocation = CreateInvocationWithSymbolInfo(invocationExpressionSyntax, semanticModel, containingBody, invocationTarget);
            var membersInOwnClass = invocation.ContainingBody.ContainingMember.ContainingClass.Members;
            invocation.InvokedImplementations.AddRange(membersInOwnClass.Where(e => e.OriginalDefinition == invocation.Defintion));
            return invocation;
            
        }
        
        private static InvocationExpressionRepresentation CreateRemoteInvocation(
            InvocationExpressionSyntax invocationExpressionSyntax, SemanticModel semanticModel, Body containingBody)
        {
            var invocationExpression = (MemberAccessExpressionSyntax) invocationExpressionSyntax.Expression;
            var invocationTarget = invocationExpression.Name;
            return CreateInvocationWithSymbolInfo(invocationExpressionSyntax, semanticModel, containingBody, invocationTarget);
        }

        private static InvocationExpressionRepresentation CreateBindingInvocation(InvocationExpressionSyntax invocationExpressionSyntax, SemanticModel semanticModel, Body containingBody)
        {
            var binding = (MemberBindingExpressionSyntax)invocationExpressionSyntax.Expression;
            var invocationTarget = binding.Name;
            return CreateInvocationWithSymbolInfo(invocationExpressionSyntax, semanticModel, containingBody, invocationTarget);
        }
        private static InvocationExpressionRepresentation CreateInvocationWithSymbolInfo(
            InvocationExpressionSyntax invocationExpressionSyntax, SemanticModel semanticModel, Body containingBody,
            SimpleNameSyntax invocationTarget)
        {
            var symbolInfo = SymbolInformationBuilder.Create(invocationTarget, semanticModel);
            return CreateInvocation(invocationExpressionSyntax, containingBody, invocationTarget, symbolInfo);
        }

        private static InvocationExpressionRepresentation CreateInvocation(
            InvocationExpressionSyntax invocationExpressionSyntax, Body containingBody, SimpleNameSyntax invocationTarget, SymbolInformation symbolInfo)
        {
            var invocationIsSynchronized = containingBody?.Implementation.IsSynchronized() ?? false;
            var invocation = new InvocationExpressionRepresentation(invocationIsSynchronized, symbolInfo, invocationExpressionSyntax, containingBody, invocationTarget, invocationExpressionSyntax.Parent is ParenthesizedLambdaExpressionSyntax);
            var arguments =
                invocationExpressionSyntax.ArgumentList.Arguments.SelectMany(e => e.GetChildren<IdentifierNameSyntax>())
                    .ToList();
            invocation.Arguments.AddRange(arguments);
            return invocation;
        }
    }
}
