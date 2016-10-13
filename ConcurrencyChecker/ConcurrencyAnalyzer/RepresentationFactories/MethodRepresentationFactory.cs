using ConcurrencyAnalyzer.Representation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConcurrencyAnalyzer.RepresentationFactories
{
    public class MethodRepresentationFactory
    {
        public static IMethodRepresentation Create(MethodDeclarationSyntax methodDeclarationSyntax, ClassRepresentation classRepresentation, SemanticModel semanticModel)
        {
            var methodRepresentation = CreatedMethod(methodDeclarationSyntax, classRepresentation, semanticModel);
            return methodRepresentation;
        }

        private static IMethodRepresentation CreatedMethod(MethodDeclarationSyntax methodDeclarationSyntax, ClassRepresentation classRepresentation, SemanticModel semanticModel)
        {
            var methodRepresentation = new MethodRepresentation(methodDeclarationSyntax, classRepresentation);
            BuildInvocationExpressions(methodRepresentation, semanticModel);
            return methodRepresentation;
        }

        private static void BuildInvocationExpressions(IMethodRepresentation methodRepresentation, SemanticModel semanticModel)
        {
            foreach (var statementSyntax in methodRepresentation.MethodImplementation.Body.Statements)
            {
                if (statementSyntax is LockStatementSyntax || statementSyntax is BlockSyntax)
                {
                    methodRepresentation.Blocks.Add(BlockRepresentationFactory.Create(statementSyntax, methodRepresentation, semanticModel));
                }
            }
        }
    }
}
