using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConcurrencyAnalyzer.SyntaxFilters
{
    public static class SyntaxNodeFilter
    {
        public static async Task<IEnumerable<ClassDeclarationSyntax>> GetClasses(SyntaxTree syntaxTree)
        {
            var root = await syntaxTree.GetRootAsync();
            return root.GetChildren<ClassDeclarationSyntax>();
        }
        public static IEnumerable<LockStatementSyntax> GetLockStatements<TSyntaxElement>(TSyntaxElement synchronizedElement) where TSyntaxElement : SyntaxNode
        {
            return synchronizedElement.GetChildren<LockStatementSyntax>();
        }

        public static IEnumerable<InvocationExpressionSyntax> GetSynchronizedInvocations(MethodDeclarationSyntax methodDeclarationSyntax)
        {
            var locksStatementsOfMethod = GetLockStatements(methodDeclarationSyntax);
            return locksStatementsOfMethod.SelectMany(e => e.GetChildren<InvocationExpressionSyntax>());
        }
    }
}
