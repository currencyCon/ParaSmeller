using ConcurrencyAnalyzer.Representation;
using ConcurrencyAnalyzer.SemanticAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConcurrencyAnalyzer.RepresentationFactories
{
    public static class PropertyRepresentationFactory
    {
        public static PropertyRepresentation Create(PropertyDeclarationSyntax propertyDeclarationSyntax, ClassRepresentation classRepresentation, SemanticModel semanticModel)
        {
            var symbolInfo = SymbolInspector.GetDeclaredSymbol<IPropertySymbol>(propertyDeclarationSyntax, semanticModel);
            var propertyRepresentation = new PropertyRepresentation(propertyDeclarationSyntax, classRepresentation, symbolInfo.OriginalDefinition.ToString());
            AddAccessors(propertyRepresentation, semanticModel);
            return propertyRepresentation;
        }

        private static void AddAccessors(PropertyRepresentation propertyRepresentation, SemanticModel semanticModel)
        {
            if (propertyRepresentation.Getter != null)
            {
                propertyRepresentation.Blocks.Add(BlockRepresentationFactory.Create(propertyRepresentation.Getter, propertyRepresentation, semanticModel));
            }
            if (propertyRepresentation.Setter != null)
            {
                propertyRepresentation.Blocks.Add(BlockRepresentationFactory.Create(propertyRepresentation.Setter, propertyRepresentation, semanticModel));
            }
        }
    }
}
