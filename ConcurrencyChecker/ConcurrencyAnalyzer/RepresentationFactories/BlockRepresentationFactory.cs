using System;
using ConcurrencyAnalyzer.Representation;
using ConcurrencyAnalyzer.SyntaxFilters;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConcurrencyAnalyzer.RepresentationFactories
{
    public static class BlockRepresentationFactory
    {
        public static IBody Create(StatementSyntax statementSyntax, IMemberWithBody parent, SemanticModel semanticModel)
        {
            if (statementSyntax is LockStatementSyntax)
            {
                var block = new LockBlock(parent, statementSyntax);
                return WithChildBodies(parent, semanticModel, block);
            }

            if (statementSyntax is BlockSyntax)
            {
                var block = new NormalBlock(parent, statementSyntax);
                return WithChildBodies(parent, semanticModel, block);
            }
            throw new NotImplementedException($"Unknow Blocktype: {statementSyntax.ToFullString()}");
        }

        private static IBody WithChildBodies(IMemberWithBody parent, SemanticModel semanticModel, IBody block)
        {
            AddInvocations(block, semanticModel);
            foreach (var syntaxNode in block.Implementation.GetDirectChildren<StatementSyntax>())
            {
                if (syntaxNode is LockStatementSyntax || syntaxNode is BlockSyntax)
                {
                    block.Blocks.Add(Create(syntaxNode, parent, semanticModel));
                }
            }
            return block;
        }

        private static void  AddInvocations(IBody body, SemanticModel semanticModel)
        {
            foreach (var invocationExpressionSyntax in body.Implementation.GetChildren<InvocationExpressionSyntax>())
            {
                if (!(invocationExpressionSyntax.Parent is ParenthesizedLambdaExpressionSyntax))
                {
                    body.InvocationExpressions.Add(InvocationExpressionRepresentationFactory.Create(invocationExpressionSyntax, semanticModel, body));
                }
            }
        } 
    }
}
