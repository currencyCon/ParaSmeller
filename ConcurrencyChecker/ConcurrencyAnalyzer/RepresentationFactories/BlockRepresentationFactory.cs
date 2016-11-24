using System;
using System.Linq;
using ConcurrencyAnalyzer.Representation;
using ConcurrencyAnalyzer.SyntaxNodeUtils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConcurrencyAnalyzer.RepresentationFactories
{
    public static class BlockRepresentationFactory
    {
        public static Body Create(StatementSyntax statementSyntax, Member parent, SemanticModel semanticModel)
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

        private static Body WithChildBodies(Member parent, SemanticModel semanticModel, Body block)
        {
            
            foreach (var syntaxNode in block.Implementation.GetDirectChildren<StatementSyntax>())
            {
                if (syntaxNode is LockStatementSyntax || syntaxNode is BlockSyntax)
                {
                    block.Blocks.Add(Create(syntaxNode, parent, semanticModel));
                }

                AddInvocations(syntaxNode, block, semanticModel);
            }
            return block;
        }

        private static void  AddInvocations(SyntaxNode node, Body body, SemanticModel semanticModel)
        {
            foreach (var invocationExpressionSyntax in node.GetChildren<InvocationExpressionSyntax>())
            {
                if (IsNotInsertedInBody(body, invocationExpressionSyntax))
                {
                    body.InvocationExpressions.Add(InvocationExpressionRepresentationFactory.Create(invocationExpressionSyntax, semanticModel, body));
                }
            }
        }

        private static bool IsNotInsertedInBody(Body body, CSharpSyntaxNode invocationExpressionSyntax)
        {
            return
                body.GetAllInvocations()
                    .All(e => e.Implementation.GetLocation() != invocationExpressionSyntax.GetLocation());
        }
    }
}
