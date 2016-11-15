
using ConcurrencyAnalyzer.Representation;
using ConcurrencyAnalyzer.SyntaxNodeUtils;

namespace ConcurrencyAnalyzer.RepresentationExtensions
{
    public static class InvocationExpressionRepresentationExtensions
    {
        public static TParent GetFirstParent<TParent>(this InvocationExpressionRepresentation invocationExpressionRepresentation)
        {
            return invocationExpressionRepresentation.Implementation.GetFirstParent<TParent>();
        }
    }
}
