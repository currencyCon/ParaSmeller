using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ParaSmellerCore.Representation;
using ParaSmellerCore.SemanticAnalysis;

namespace ParaSmellerCore.RepresentationFactories
{
    public static class MethodRepresentationFactory
    {

        public static MethodRepresentation Create(MethodDeclarationSyntax methodDeclarationSyntax, ClassRepresentation classRepresentation, SemanticModel semanticModel)
        {
            var symbolInfo = SymbolInspector.GetDeclaredSymbol<IMethodSymbol>(methodDeclarationSyntax, semanticModel);
            var methodRepresentation = new MethodRepresentation(methodDeclarationSyntax, classRepresentation, symbolInfo.OriginalDefinition.ToString(), semanticModel);
            return WithBaseBody(methodRepresentation, semanticModel);
        }

        public static MethodRepresentation Create(MethodDeclarationSyntax methodDeclarationSyntax, InterfaceRepresentation interfaceRepresentation, SemanticModel semanticModel)
        {
            var symbolInfo = SymbolInspector.GetDeclaredSymbol<IMethodSymbol>(methodDeclarationSyntax, semanticModel);
            var methodRepresentation = new MethodRepresentation(methodDeclarationSyntax, interfaceRepresentation, symbolInfo.OriginalDefinition.ToString(), semanticModel);
            return WithBaseBody(methodRepresentation, semanticModel);
        }

        private static MethodRepresentation WithBaseBody(MethodRepresentation methodRepresentation, SemanticModel semanticModel)
        {
            if (methodRepresentation.Implementation.Body != null)
            {
                var baseBody = BlockRepresentationFactory.Create(methodRepresentation.Implementation.Body,
                    methodRepresentation, semanticModel);
                methodRepresentation.Blocks.Add(baseBody);
            }
            return methodRepresentation;
        }
    }
}
