using System.Linq;
using ConcurrencyAnalyzer.Representation;
using ConcurrencyAnalyzer.SyntaxFilters;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConcurrencyAnalyzer.RepresentationFactories
{
    public class MethodRepresentationFactory
    {
        public static IMethodRepresentation Create(MethodDeclarationSyntax methodDeclarationSyntax, ClassRepresentation classRepresentation)
        {
            var isSynchronized = SyntaxNodeFilter.GetLockStatements(methodDeclarationSyntax).Any();
            if (isSynchronized)
            {
                return CreateSynchronizedMethod(methodDeclarationSyntax, classRepresentation);
            }
            return CreateUnsychronizedMethod(methodDeclarationSyntax, classRepresentation);
        }

        private static IMethodRepresentation CreateUnsychronizedMethod(MethodDeclarationSyntax methodDeclarationSyntax, ClassRepresentation classRepresentation)
        {
            var methodRepresentation =  new UnSynchronizedMethodRepresentation(methodDeclarationSyntax, classRepresentation);
            BuildInvocationExpressions(methodRepresentation);
            return methodRepresentation;
        }

        private static IMethodRepresentation CreateSynchronizedMethod(MethodDeclarationSyntax methodDeclarationSyntax, ClassRepresentation classRepresentation)
        {
            var methodRepresentation = new SynchronizedMethodRepresentation(methodDeclarationSyntax, classRepresentation);
            BuildInvocationExpressions(methodRepresentation);
            return methodRepresentation;
        }

        private static void BuildInvocationExpressions(IMethodRepresentation methodRepresentation)
        {
            var invocationExpressions = methodRepresentation.MethodImplementation.GetChildren<InvocationExpressionSyntax>();
            foreach (var invocationExpressionSyntax in invocationExpressions)
            {
                methodRepresentation.InvocationExpressions.Add(InvocationExpressionRepresentationFactory.Create(invocationExpressionSyntax, methodRepresentation.ContainingClass, methodRepresentation));
            }
        }
    }
}
