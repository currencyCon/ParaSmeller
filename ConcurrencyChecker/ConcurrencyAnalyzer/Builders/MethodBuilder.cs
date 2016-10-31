
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConcurrencyAnalyzer.Builders
{
    public static class MethodBuilder
    {
        private const int OneTab = 1;

        public static MethodDeclarationSyntax BuildLockedMethod(MethodDeclarationSyntax method, ExpressionSyntax defaultLockObject)
        {
            var body = method.Body;
            foreach (var statementSyntax in body.Statements)
            {
                body = body.ReplaceNode(statementSyntax, SyntaxFormatter.AddIndention(statementSyntax, OneTab));
            }
            body = body.ReplaceToken(body.CloseBraceToken, SyntaxFormatter.AddIndention(body.CloseBraceToken, OneTab));
            var lockStatementBlock = LockBuilder.BuildLockBlock(body, defaultLockObject);
            var newMethod = method.ReplaceNode(method, method.WithBody(lockStatementBlock));
            return newMethod;
        }

        public static MethodDeclarationSyntax BuildLockedMethod(MethodDeclarationSyntax method)
        {
            return BuildLockedMethod(method, LockBuilder.DefaultLockObject());
        }

    }
}
