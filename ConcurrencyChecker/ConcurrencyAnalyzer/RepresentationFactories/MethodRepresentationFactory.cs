using System.Linq;
using ConcurrencyAnalyzer.Representation;
using ConcurrencyAnalyzer.SyntaxFilters;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConcurrencyAnalyzer.RepresentationFactories
{
    public class MethodRepresentationFactory
    {
        public static IMethodRepresentation Create(MethodDeclarationSyntax methodDeclarationSyntax, ClassRepresentation classRepresentation, SemanticModel semanticModel)
        {
            IMethodRepresentation methodRepresentation = null;
            var isSynchronized = SyntaxNodeFilter.GetLockStatements(methodDeclarationSyntax).Any();
            if (isSynchronized)
            {
                methodRepresentation = CreateSynchronizedMethod(methodDeclarationSyntax, classRepresentation,
                    semanticModel);
            }
            else
            {
                methodRepresentation = CreateUnsychronizedMethod(methodDeclarationSyntax, classRepresentation, semanticModel);

            }
            
            foreach (var statementSyntax in methodRepresentation.MethodImplementation.Body.Statements)
            {
                if (statementSyntax is LockStatementSyntax || statementSyntax is BlockSyntax)
                {
                    methodRepresentation.Blocks.Add(BlockRepresentationFactory.Create(statementSyntax, methodRepresentation, semanticModel));
                }
            }
            return methodRepresentation;
        }

        private static IMethodRepresentation CreateUnsychronizedMethod(MethodDeclarationSyntax methodDeclarationSyntax, ClassRepresentation classRepresentation, SemanticModel semanticModel)
        {
            var methodRepresentation =  new UnSynchronizedMethodRepresentation(methodDeclarationSyntax, classRepresentation);
            BuildInvocationExpressions(methodRepresentation, semanticModel);
            return methodRepresentation;
        }

        private static IMethodRepresentation CreateSynchronizedMethod(MethodDeclarationSyntax methodDeclarationSyntax, ClassRepresentation classRepresentation, SemanticModel semanticModel)
        {
            var methodRepresentation = new SynchronizedMethodRepresentation(methodDeclarationSyntax, classRepresentation);
            BuildInvocationExpressions(methodRepresentation, semanticModel);
            return methodRepresentation;
        }

        private static void BuildInvocationExpressions(IMethodRepresentation methodRepresentation, SemanticModel semanticModel)
        {
/*            var invocationExpressions = methodRepresentation.MethodImplementation.GetChildren<InvocationExpressionSyntax>();
            foreach (var invocationExpressionSyntax in invocationExpressions)
            {
                methodRepresentation.InvocationExpressions.Add(InvocationExpressionRepresentationFactory.Create(invocationExpressionSyntax, methodRepresentation.ContainingClass, methodRepresentation, semanticModel));
            }*/
        }
    }
}
