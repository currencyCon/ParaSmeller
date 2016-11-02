
using System.Collections.Generic;
using ConcurrencyAnalyzer.Representation;
using ConcurrencyAnalyzer.SyntaxNodeUtils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ConcurrencyAnalyzer.RepresentationExtensions
{
    public static class InvocationExpressionRepresentationExtensions
    {
        public static TChild GetFirstChild<TChild>(this InvocationExpressionRepresentation invocationExpressionRepresentation)
        {
            return invocationExpressionRepresentation.Implementation.GetFirstChild<TChild>();
        }
        public static IEnumerable<TChildren> GetChildren<TChildren>(this InvocationExpressionRepresentation invocationExpressionRepresentation)
        {
            return invocationExpressionRepresentation.Implementation.GetChildren<TChildren>();
        }

        public static TParent GetFirstParent<TParent>(this InvocationExpressionRepresentation invocationExpressionRepresentation)
        {
            return invocationExpressionRepresentation.Implementation.GetFirstParent<TParent>();
        }

        public static IEnumerable<TParents> GetParents<TParents>(this InvocationExpressionRepresentation invocationExpressionRepresentation)
        {
            return invocationExpressionRepresentation.Implementation.GetParents<TParents>();
        }

        public static IMethodSymbol GetMethodSymbol(this InvocationExpressionRepresentation invocationExpressionRepresentation, CompilationAnalysisContext context)
        {
            return invocationExpressionRepresentation.Implementation.GetMethodSymbol(context);
        }

        
    }
}
