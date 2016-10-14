

using System.Collections.Generic;
using ConcurrencyAnalyzer.Representation;
using ConcurrencyAnalyzer.SyntaxFilters;

namespace ConcurrencyAnalyzer.RepresentationExtensions
{
    public static class MethodExtensions
    {
        public static IEnumerable<TChildren> GetChildren<TChildren>(this IMethodRepresentation node)
        {
            if (node?.MethodImplementation == null)
            {
                return new List<TChildren>();
            }
            return node.MethodImplementation.GetChildren<TChildren>();
        }
    }
}
