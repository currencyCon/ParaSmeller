using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConcurrencyAnalyzer.Representation;
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

        public static IEnumerable<LockStatementSyntax> GetLockStatements<TSyntaxElement>(IEnumerable<TSyntaxElement> synchronizedElements) where TSyntaxElement : SyntaxNode
        {
            return synchronizedElements.SelectMany(GetLockStatements);
        }
        public static IEnumerable<InvocationExpressionSyntax> GetSynchronizedInvocations(MethodDeclarationSyntax methodDeclarationSyntax)
        {
            var locksStatementsOfMethod = GetLockStatements(methodDeclarationSyntax);
            return locksStatementsOfMethod.SelectMany(e => e.GetChildren<InvocationExpressionSyntax>());
        }

        public static IEnumerable<IdentifierNameSyntax> GetIdentifiersInLocks(IEnumerable<IBody> bodies)
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

        public static IEnumerable<IInvocationExpressionRepresentation> GetInvocationsInLocks(IEnumerable<IBody> bodies)
        {
            IEnumerable<IInvocationExpressionRepresentation> invocations = new List<IInvocationExpressionRepresentation>();
            foreach (var body in bodies)
            {
                if (body.IsSynchronized)
                {
                    invocations = invocations.Concat(body.InvocationExpressions);
                }
                invocations = invocations.Concat(GetInvocationsInLocks(body.Blocks));
            }
            return invocations;
        }
    }
}
