
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConcurrencyAnalyzer.SyntaxFilters
{
    public static class SyntaxNodeExtensions
    {
        public static TChild GetFirstChild<TChild>(this SyntaxNode node)
        {
            return GetChildren<TChild>(node).FirstOrDefault();
        }
        public static IEnumerable<TChildren> GetChildren<TChildren>(this SyntaxNode node)
        {
            if (node == null)
            {
                return new List<TChildren>();
            }
            return node.DescendantNodesAndSelf().OfType<TChildren>();
        }

        public static TParent GetFirstParent<TParent>(this SyntaxNode node)
        {
            return GetParents<TParent>(node).FirstOrDefault();
        }

        public static IEnumerable<TParents> GetParents<TParents>(this SyntaxNode node)
        {
            if (node == null)
            {
                return new List<TParents>();
            }
            return node.AncestorsAndSelf().OfType<TParents>();
        }

        public static IEnumerable<TChildren> GetDirectChildren<TChildren>(this SyntaxNode node)
        {
            return node.ChildNodes().OfType<TChildren>();
        }

        public static bool IsSynchronized(this SyntaxNode node)
        {
            return node.GetChildren<LockStatementSyntax>().Any();
        }

        public static IEnumerable<MemberAccessExpressionSyntax> GetInvocationExpression(this SyntaxNode node, string clazz, string methodName)
        {
            return node.GetChildren<MemberAccessExpressionSyntax>().Where(e => e.Expression.ToString() == clazz && e.Name.ToString() == methodName);
        } 
    }
}
