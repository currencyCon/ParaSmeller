
using System.Collections.Generic;
using ConcurrencyAnalyzer.Representation;
using ConcurrencyAnalyzer.SyntaxFilters;

namespace ConcurrencyAnalyzer.RepresentationExtensions
{
    public static class InvocationExpressionRepresentationExtensions
    {
        public static TChild GetFirstChild<TChild>(this IInvocationExpressionRepresentation invocationExpressionRepresentation)
        {
            return invocationExpressionRepresentation.Implementation.GetFirstChild<TChild>();
        }
        public static IEnumerable<TChildren> GetChildren<TChildren>(this IInvocationExpressionRepresentation invocationExpressionRepresentation)
        {
            return invocationExpressionRepresentation.Implementation.GetChildren<TChildren>();
        }

        public static TParent GetFirstParent<TParent>(this IInvocationExpressionRepresentation invocationExpressionRepresentation)
        {
            return invocationExpressionRepresentation.Implementation.GetFirstParent<TParent>();
        }

        public static IEnumerable<TParents> GetParents<TParents>(this IInvocationExpressionRepresentation invocationExpressionRepresentation)
        {
            return invocationExpressionRepresentation.Implementation.GetParents<TParents>();
        }
    }
}
