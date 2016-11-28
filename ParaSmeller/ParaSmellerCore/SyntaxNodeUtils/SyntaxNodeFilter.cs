using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ParaSmellerCore.Representation;

namespace ParaSmellerCore.SyntaxNodeUtils
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

        public static IEnumerable<SyntaxToken> GetConstantDeclarations(SyntaxNode node)
        {
            return node.GetChildren<LocalDeclarationStatementSyntax>()
                .Select(e => e.Declaration)
                .Where(
                    a =>
                        a.GetChildren<EqualsValueClauseSyntax>()
                            .Any(z => z.Value is LiteralExpressionSyntax))
                .SelectMany(h => h.Variables.Select(u => u.Identifier));
        }

        public static bool ReturnsConstantValue(SyntaxNode node)
        {
            var returnStatement = node?.GetFirstChild<ReturnStatementSyntax>();
            var returnValue = returnStatement?.Expression;
            if (returnValue != null)
            {
                if (returnValue is LiteralExpressionSyntax)
                {
                    return true;
                }
                if (returnValue is IdentifierNameSyntax)
                {
                    var localConstantDeclarations = GetConstantDeclarations(node).Select(e => e.Text);
                    if (localConstantDeclarations.Contains(((IdentifierNameSyntax) returnValue).Identifier.Text))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
