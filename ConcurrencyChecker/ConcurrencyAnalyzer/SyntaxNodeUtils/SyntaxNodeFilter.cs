using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConcurrencyAnalyzer.Representation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConcurrencyAnalyzer.SyntaxNodeUtils
{
    public static class SyntaxNodeFilter
    {
        public static async Task<IEnumerable<ClassDeclarationSyntax>> GetClasses(SyntaxTree syntaxTree)
        {
            var root = await syntaxTree.GetRootAsync();
            return root.GetChildren<ClassDeclarationSyntax>();
        }
        public static async Task<IEnumerable<InterfaceDeclarationSyntax>> GetInterfaces(SyntaxTree syntaxTree)
        {
            var root = await syntaxTree.GetRootAsync();
            return root.GetChildren<InterfaceDeclarationSyntax>();
        }

        public static IEnumerable<LockStatementSyntax> GetLockStatements<TSyntaxElement>(TSyntaxElement synchronizedElement) where TSyntaxElement : SyntaxNode
        {
            return synchronizedElement.GetChildren<LockStatementSyntax>();
        }

        public static IEnumerable<IdentifierNameSyntax> GetIdentifiersInLocks(IEnumerable<Body> bodies)
        {
            IEnumerable<IdentifierNameSyntax> identifiers = new List<IdentifierNameSyntax>();
            foreach (var body in bodies)
            {
                if (body.IsSynchronized)
                {
                    var lockStatement = body.Implementation as LockStatementSyntax;
                    if (lockStatement != null)
                    {
                        identifiers = identifiers.Concat(lockStatement.Statement.GetChildren<IdentifierNameSyntax>());
                    }
                }
                identifiers = identifiers.Concat(GetIdentifiersInLocks(body.Blocks));
            }
            return identifiers;
        }
    }
}
