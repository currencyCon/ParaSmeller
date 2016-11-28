using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ParaSmellerCore.Representation;
using ParaSmellerCore.SyntaxNodeUtils;

namespace ParaSmellerCore.RepresentationFactories
{
    public static class InterfaceRepresentationFactory
    {
        public static InterfaceRepresentation Create(InterfaceDeclarationSyntax syntaxTree, SemanticModel semanticModel)
        {
            var interfaceRepresentation = new InterfaceRepresentation(syntaxTree, semanticModel);
            AddMethods(interfaceRepresentation, semanticModel);
            AddProperties(interfaceRepresentation, semanticModel);
            return interfaceRepresentation;
        }
        
        private static void AddProperties(InterfaceRepresentation interfaceRepresentation, SemanticModel semanticModel)
        {
            var properties = interfaceRepresentation.Implementation.GetChildren<PropertyDeclarationSyntax>();
            foreach (var propertyDeclarationSyntax in properties)
            {
                interfaceRepresentation.Members.Add(PropertyRepresentationFactory.Create(propertyDeclarationSyntax, interfaceRepresentation, semanticModel));
            }
        }

        private static void AddMethods(InterfaceRepresentation interfaceRepresentation, SemanticModel semanticModel)
        {
            var methods = interfaceRepresentation.Implementation.GetChildren<MethodDeclarationSyntax>();
            foreach (var methodDeclarationSyntax in methods)
            {
                interfaceRepresentation.Members.Add(MethodRepresentationFactory.Create(methodDeclarationSyntax, interfaceRepresentation, semanticModel));
            }
        }
    }
}
