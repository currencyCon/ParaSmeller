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
            IBody block = null;
            if (statementSyntax is LockStatementSyntax)
            {
                block = CreateLockBlock((LockStatementSyntax) statementSyntax, parent);
                
            }

            if (statementSyntax is BlockSyntax)
            {
                block = CreateBlock((BlockSyntax) statementSyntax, parent);
            }
            if (block != null)
            {
                AddInvocations(block, semanticModel);
                foreach (var syntaxNode in block.Implementation.GetDirectChildren<StatementSyntax>())
                {
                    if (syntaxNode is LockStatementSyntax || syntaxNode is BlockSyntax)
                    {
                        block.Blocks.Add(Create(syntaxNode, parent, semanticModel));
                    }
                }
            }
            return block;
        }

        private static IBody CreateBlock(BlockSyntax statementSyntax, IMemberWithBody parent)
        {
            return new NormalBlock(parent, statementSyntax);

        }

        private static IBody CreateLockBlock(LockStatementSyntax statementSyntax, IMemberWithBody parent)
        {
            return new LockBlock(parent, statementSyntax);
        }

        private static void  AddInvocations(IBody body, SemanticModel semanticModel)
        {
            foreach (var invocationExpressionSyntax in body.Implementation.GetChildren<InvocationExpressionSyntax>())
            {
                body.InvocationExpressions.Add(InvocationExpressionRepresentationFactory.Create(invocationExpressionSyntax, semanticModel, body));
            }
        } 
    }
}
