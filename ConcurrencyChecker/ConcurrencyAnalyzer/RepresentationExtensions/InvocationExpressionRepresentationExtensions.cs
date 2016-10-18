
using System.Collections.Generic;
using ConcurrencyAnalyzer.Representation;
using ConcurrencyAnalyzer.SyntaxFilters;

namespace ConcurrencyAnalyzer.RepresentationExtensions
{
    public static class InvocationExpressionRepresentationExtensions
    {
        public static TChild GetFirstChild<TChild>(this IInvocationExpression invocationExpression)
        {
            return invocationExpression.Implementation.GetFirstChild<TChild>();
        }
        public static IEnumerable<TChildren> GetChildren<TChildren>(this IInvocationExpression invocationExpression)
        {
            return invocationExpression.Implementation.GetChildren<TChildren>();
        }

        public static TParent GetFirstParent<TParent>(this IInvocationExpression invocationExpression)
        {
            return invocationExpression.Implementation.GetFirstParent<TParent>();
        }

        public static IEnumerable<TParents> GetParents<TParents>(this IInvocationExpression invocationExpression)
        {
            return invocationExpression.Implementation.GetParents<TParents>();
        }
    }
}
