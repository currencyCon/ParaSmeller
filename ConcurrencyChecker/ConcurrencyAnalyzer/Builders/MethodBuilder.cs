
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConcurrencyAnalyzer.Builders
{
    public class MethodBuilder
    {
        public static MethodDeclarationSyntax BuildLockedMethod(MethodDeclarationSyntax method, ExpressionSyntax defaultLockObject = null)
        {
            var body = method.Body;
            foreach (var statementSyntax in body.Statements)
            {
                body = body.ReplaceNode(statementSyntax, SyntaxFormatter.AddIndention(statementSyntax, 1));
            }
            body = body.ReplaceToken(body.CloseBraceToken, SyntaxFormatter.AddIndention(body.CloseBraceToken, 1));
            var lockStatementBlock = LockBuilder.BuildLockBlock(body, defaultLockObject);
            var newMeth = method.ReplaceNode(method, method.WithBody(lockStatementBlock));
            return newMeth;
        }
    }
}
