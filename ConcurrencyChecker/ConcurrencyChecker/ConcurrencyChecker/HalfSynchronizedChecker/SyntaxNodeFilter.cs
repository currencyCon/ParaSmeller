using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConcurrencyChecker.HalfSynchronizedChecker
{
    public class SyntaxNodeFilter
    {
        public static IEnumerable<PropertyDeclarationSyntax> GetSynchronizedProperties(IEnumerable<PropertyDeclarationSyntax> properties)
        {
            var synchronizedProperties =
                properties.Where(e => e.Modifiers.Any(a => a.IsKind(SyntaxKind.PublicKeyword)))
                    .Where(e => e.DescendantNodes().OfType<LockStatementSyntax>().Any()).ToList();
            return synchronizedProperties;
        }

        public static IEnumerable<MethodDeclarationSyntax> GetSynchronizedMethods(IEnumerable<MethodDeclarationSyntax> methods)
        {
            var synchronizedMethods = methods.Where(e => e.Modifiers.Any(a => a.IsKind(SyntaxKind.PublicKeyword)))
                .Where(e => e.DescendantNodes().OfType<LockStatementSyntax>().Any()).ToList();
            return synchronizedMethods;
        }

        public static IEnumerable<MethodDeclarationSyntax> GetUnsynchronizedMethods(IEnumerable<MethodDeclarationSyntax> methods)
        {
            return methods.Where(e => e.Modifiers.Any(a => a.IsKind(SyntaxKind.PublicKeyword)))
                .Where(e => !e.DescendantNodes().OfType<LockStatementSyntax>().Any()).ToList();
        }

        public static IEnumerable<PropertyDeclarationSyntax> GetUnsynchronizedProperties(IEnumerable<PropertyDeclarationSyntax> properties)
        {
            var unsyncedProperties = properties.Where(e => e.Modifiers.Any(a => a.IsKind(SyntaxKind.PublicKeyword)))
                .Where(e => !e.DescendantNodes().OfType<LockStatementSyntax>().Any()).ToList();
            return unsyncedProperties;
        }

        private static IEnumerable<SyntaxToken> GetIdentifiersUsedInLocks(IEnumerable<LockStatementSyntax> locksStatementsOfProperties)
        {
            var identifiersUsedInLockStatements =
                locksStatementsOfProperties.ToList()
                    .SelectMany(a => a.DescendantNodes().OfType<IdentifierNameSyntax>())
                    .Select(e => e.Identifier)
                    .Distinct()
                    .ToList();
            return identifiersUsedInLockStatements;
        }

        private static IEnumerable<LockStatementSyntax> GetLockStatements<TSyntaxElement>(IEnumerable<TSyntaxElement> synchronizedElements) where TSyntaxElement : SyntaxNode
        {
            return synchronizedElements.SelectMany(GetLockStatements).ToList();
        }
        private static IEnumerable<LockStatementSyntax> GetLockStatements<TSyntaxElement>(TSyntaxElement synchronizedElement) where TSyntaxElement : SyntaxNode
        {
            return synchronizedElement.DescendantNodes().OfType<LockStatementSyntax>().ToList();
        }
        public static IEnumerable<string> GetIdentifiersInLockStatements(IEnumerable<SyntaxNode> synchronizedMethods)
        {
            var locksStatementsOfProperties = GetLockStatements(synchronizedMethods);
            return GetIdentifiersUsedInLocks(locksStatementsOfProperties).Select(e => e.Text);
        }

        public static IEnumerable<PropertyDeclarationSyntax> GetPropertiesInSynchronizedMethods(IEnumerable<MethodDeclarationSyntax> synchronizedMethods,
            IEnumerable<PropertyDeclarationSyntax> properties)
        {
            var x = GetIdentifiersInLockStatements(synchronizedMethods);
            return properties.Where(e => x.Contains(e.Identifier.Text));
        }

        private static SyntaxNode GetLockObject(LockStatementSyntax lockStatementSyntax)
        {
            return lockStatementSyntax.Expression;
        }

        public static IEnumerable<SyntaxNode> GetLockObjects<TSyntaxElement>(
            TSyntaxElement syntaxNode) where TSyntaxElement : SyntaxNode
        {
            return GetLockStatements(syntaxNode).Select(GetLockObject);
        }

        public static IEnumerable<SyntaxNode> GetLockObjects<TSyntaxElement>(IEnumerable<TSyntaxElement> syntaxNodes)
            where TSyntaxElement : SyntaxNode
        {
            return syntaxNodes.SelectMany(GetLockObjects);
        }

    }
}
