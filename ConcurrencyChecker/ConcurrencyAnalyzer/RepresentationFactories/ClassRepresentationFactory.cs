using ConcurrencyAnalyzer.Representation;
using ConcurrencyAnalyzer.SyntaxFilters;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConcurrencyAnalyzer.RepresentationFactories
{
    public class ClassRepresentationFactory
    {
        public static ClassRepresentation Create(ClassDeclarationSyntax syntaxTree, SemanticModel semanticModel)
        {
            var classRepresentation = new ClassRepresentation(syntaxTree);
            AddMethods(classRepresentation, semanticModel);
            AddProperties(classRepresentation, semanticModel);
            return classRepresentation;
        }

        private static void AddProperties(ClassRepresentation classRepresentation, SemanticModel semanticModel)
        {
            var properties = classRepresentation.ClassDeclarationSyntax.GetChildren<PropertyDeclarationSyntax>();
            foreach (var propertyDeclarationSyntax in properties)
            {
                classRepresentation.Members.Add(PropertyRepresentationFactory.Create(propertyDeclarationSyntax, classRepresentation, semanticModel));
            }
        }

        private static void AddMethods(ClassRepresentation classRepresentation, SemanticModel semanticModel)
        {
            var methods = classRepresentation.ClassDeclarationSyntax.GetChildren<MethodDeclarationSyntax>();
            foreach (var methodDeclarationSyntax in methods)
            {
                classRepresentation.Members.Add(MethodRepresentationFactory.Create(methodDeclarationSyntax, classRepresentation, semanticModel));
            }
        }
    }
}
