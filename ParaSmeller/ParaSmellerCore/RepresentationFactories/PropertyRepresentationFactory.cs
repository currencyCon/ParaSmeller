using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ParaSmellerCore.Representation;
using ParaSmellerCore.SemanticAnalysis;

namespace ParaSmellerCore.RepresentationFactories
{
    public static class PropertyRepresentationFactory
    {
        public static PropertyRepresentation Create(PropertyDeclarationSyntax propertyDeclarationSyntax, ClassRepresentation classRepresentation, SemanticModel semanticModel)
        {
            var symbolInfo = SymbolInspector.GetDeclaredSymbol<IPropertySymbol>(propertyDeclarationSyntax, semanticModel);
            var propertyRepresentation = new PropertyRepresentation(propertyDeclarationSyntax, classRepresentation, symbolInfo.OriginalDefinition.ToString(), semanticModel);
            return WithAccessors(propertyRepresentation, semanticModel);
        }

        public static PropertyRepresentation Create(PropertyDeclarationSyntax propertyDeclarationSyntax, InterfaceRepresentation interfaceRepresentation, SemanticModel semanticModel)
        {
            var symbolInfo = SymbolInspector.GetDeclaredSymbol<IPropertySymbol>(propertyDeclarationSyntax, semanticModel);
            var propertyRepresentation = new PropertyRepresentation(propertyDeclarationSyntax, interfaceRepresentation, symbolInfo.OriginalDefinition.ToString(), semanticModel);
            return WithAccessors(propertyRepresentation, semanticModel);
        }

        private static PropertyRepresentation WithAccessors(PropertyRepresentation propertyRepresentation, SemanticModel semanticModel)
        {
            if (propertyRepresentation.Getter != null)
            {
                propertyRepresentation.Blocks.Add(BlockRepresentationFactory.Create(propertyRepresentation.Getter, propertyRepresentation, semanticModel));
            }
            if (propertyRepresentation.Setter != null)
            {
                propertyRepresentation.Blocks.Add(BlockRepresentationFactory.Create(propertyRepresentation.Setter, propertyRepresentation, semanticModel));
            }
            return propertyRepresentation;
        }
    }
}
