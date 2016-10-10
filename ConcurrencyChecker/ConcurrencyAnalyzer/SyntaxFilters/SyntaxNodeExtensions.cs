
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace ConcurrencyAnalyzer.SyntaxFilters
{
    public static class SyntaxNodeExtensions
    {
        public static IEnumerable<TChildren> GetChildren<TChildren>(this SyntaxNode node)
        {
            return node.DescendantNodesAndSelf().OfType<TChildren>();
        }
    }
}
