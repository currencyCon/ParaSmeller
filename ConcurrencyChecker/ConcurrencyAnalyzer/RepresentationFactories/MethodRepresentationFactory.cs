using System.Linq;
using ConcurrencyAnalyzer.Representation;
using ConcurrencyAnalyzer.SyntaxFilters;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConcurrencyAnalyzer.RepresentationFactories
{
    public class MethodRepresentationFactory
    {
        private const string ThreadStartDefintion = "System.Threading.Thread.Run()";

        public static IMethodRepresentation Create(MethodDeclarationSyntax methodDeclarationSyntax, ClassRepresentation classRepresentation, SemanticModel semanticModel)
        {
            var methodRepresentation = CreatedMethod(methodDeclarationSyntax, classRepresentation, semanticModel);
            return methodRepresentation;
        }

        private static IMethodRepresentation CreatedMethod(MethodDeclarationSyntax methodDeclarationSyntax, ClassRepresentation classRepresentation, SemanticModel semanticModel)
        {
            var methodRepresentation = new MethodRepresentation(methodDeclarationSyntax, classRepresentation);
            BuildInvocationExpressions(methodRepresentation, semanticModel);
            AddDirectInvcocations(methodRepresentation, semanticModel);
            return methodRepresentation;
        }

        private static void AddDirectInvcocations(IMethodRepresentation methodRepresentation, SemanticModel semanticModel)
        {
            foreach (var invocationExpressionSyntax in methodRepresentation.MethodImplementation.Body.Statements.Where(e => !(e is LockStatementSyntax) && ! (e is BlockSyntax)).SelectMany(e => e.GetChildren<InvocationExpressionSyntax>()))
            {
                var methodSymbol = semanticModel.GetSymbolInfo(invocationExpressionSyntax).Symbol;

/*                if (methodSymbol == null)
                {
                    continue;
                }*/
/*                if (methodSymbol.OriginalDefinition.ToString() != ThreadStartDefintion)
                {
                    continue;
                }*/

                methodRepresentation.InvocationExpressions.Add(InvocationExpressionRepresentationFactory.Create(invocationExpressionSyntax, semanticModel));
            }
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
