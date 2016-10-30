
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConcurrencyAnalyzer.Builders
{
    public static class MethodBuilder
    {
        public static MethodDeclarationSyntax BuildLockedMethod(MethodDeclarationSyntax method, ExpressionSyntax defaultLockObject)
        {
            var body = RebuildBody(method);
            var lockStatementBlock = LockBuilder.BuildLockBlock(body, defaultLockObject);
            return BuildMethod(method, lockStatementBlock);
        }

        private static MethodDeclarationSyntax BuildMethod(MethodDeclarationSyntax method, BlockSyntax lockStatementBlock)
        {
            var newMeth = method.ReplaceNode(method, method.WithBody(lockStatementBlock));
            return newMeth;
        }

        private static BlockSyntax RebuildBody(BaseMethodDeclarationSyntax method)
        {
            var body = method.Body;
            foreach (var statementSyntax in body.Statements)
            {
                body = body.ReplaceNode(statementSyntax, SyntaxFormatter.AddIndention(statementSyntax, 1));
            }
            body = body.ReplaceToken(body.CloseBraceToken, SyntaxFormatter.AddIndention(body.CloseBraceToken, 1));
            return body;
        }

        public static MethodDeclarationSyntax BuildLockedMethod(MethodDeclarationSyntax method)
        {
            var body = RebuildBody(method);
            var lockStatementBlock = LockBuilder.BuildLockBlock(body);
            return BuildMethod(method, lockStatementBlock);

        }
    }
}
