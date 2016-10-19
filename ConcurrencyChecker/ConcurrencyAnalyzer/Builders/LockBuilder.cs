using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConcurrencyAnalyzer.Builders
{
    public class LockBuilder
    {
        public static ExpressionSyntax DefaultLockObject()
        {
            return SyntaxFactory.ThisExpression();
        }
        public static BlockSyntax BuildLockBlock(StatementSyntax body, ExpressionSyntax lockObject = null)
        {
            var lockObj = lockObject ?? DefaultLockObject();
            var openParans = SyntaxFactory.Token(SyntaxKind.OpenParenToken);
            var closingParans = SyntaxFactory.Token(SyntaxKind.CloseParenToken);
            var lockStatement = SyntaxFactory.LockStatement(SyntaxFactory.Token(SyntaxKind.LockKeyword),
                openParans,
                lockObj,
                closingParans, body);
            var lockStatementBlock =
                SyntaxFactory.Block(lockStatement);
            return lockStatementBlock;
        }
    }
}
