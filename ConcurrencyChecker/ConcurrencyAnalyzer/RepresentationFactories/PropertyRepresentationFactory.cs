﻿using ConcurrencyAnalyzer.Representation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConcurrencyAnalyzer.RepresentationFactories
{
    public static class PropertyRepresentationFactory
    {
        public static PropertyRepresentation Create(PropertyDeclarationSyntax propertyDeclarationSyntax, ClassRepresentation classRepresentation, SemanticModel semanticModel)
        {
            return CreateProperty(propertyDeclarationSyntax, classRepresentation, semanticModel);
        }


        private static PropertyRepresentation CreateProperty(PropertyDeclarationSyntax propertyDeclarationSyntax, ClassRepresentation classRepresentation, SemanticModel semanticModel)
        {
            var propertyRepresentation = new PropertyRepresentation(propertyDeclarationSyntax, classRepresentation);
            BuildInvocationExpressions(propertyRepresentation, semanticModel);
            return propertyRepresentation;
        }

        private static void BuildInvocationExpressions(PropertyRepresentation propertyRepresentation, SemanticModel semanticModel)
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
