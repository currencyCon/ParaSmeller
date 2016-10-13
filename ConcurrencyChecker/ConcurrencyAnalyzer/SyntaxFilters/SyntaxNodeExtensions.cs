
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConcurrencyAnalyzer.SyntaxFilters
{
    public static class SyntaxNodeExtensions
    {
        public static IEnumerable<TChildren> GetChildren<TChildren>(this SyntaxNode node)
        {
            if (node == null)
            {
                return new List<TChildren>();
            }
            return node.DescendantNodesAndSelf().OfType<TChildren>();
        }

        public static IEnumerable<TChildren> GetDirectChildren<TChildren>(this SyntaxNode node)
        {
            return node.ChildNodes().OfType<TChildren>();
        }

        public static bool IsSynchronized(this SyntaxNode node)
        {
            return node.GetChildren<LockStatementSyntax>().Any();
        }
    }
}
