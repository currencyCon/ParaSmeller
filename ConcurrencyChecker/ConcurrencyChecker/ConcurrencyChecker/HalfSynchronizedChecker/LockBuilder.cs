using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConcurrencyChecker.HalfSynchronizedChecker
{
    public class LockBuilder
    {
        public static BlockSyntax BuildLockBlock(StatementSyntax body)
        {
            return BuildLockBlock(body, SyntaxFactory.ThisExpression());
        }

        public static BlockSyntax BuildLockBlock(StatementSyntax body, ExpressionSyntax lockObject)
        {
            var openParans = SyntaxFactory.Token(SyntaxKind.OpenParenToken);
            var closingParans = SyntaxFactory.Token(SyntaxKind.CloseParenToken);
            var lockStatement = SyntaxFactory.LockStatement(SyntaxFactory.Token(SyntaxKind.LockKeyword),
                openParans,
                lockObject,
                closingParans, body);
            var lockStatementBlock =
                SyntaxFactory.Block(lockStatement);
            return lockStatementBlock;
        }
    }
}
