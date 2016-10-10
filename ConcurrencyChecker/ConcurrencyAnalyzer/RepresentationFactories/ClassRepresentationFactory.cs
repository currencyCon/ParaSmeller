using ConcurrencyAnalyzer.Representation;
using ConcurrencyAnalyzer.SyntaxFilters;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConcurrencyAnalyzer.RepresentationFactories
{
    public class ClassRepresentationFactory
    {
        public static ClassRepresentation Create(ClassDeclarationSyntax syntaxTree)
        {
            var classRepresentation = new ClassRepresentation(syntaxTree);
            AddMethods(classRepresentation);
            return classRepresentation;
        }

        private static void AddMethods(ClassRepresentation classRepresentation)
        {
            var methods = classRepresentation.ClassDeclarationSyntax.GetChildren<MethodDeclarationSyntax>();
            foreach (var methodDeclarationSyntax in methods)
            {
                classRepresentation.Methods.Add(MethodRepresentationFactory.Create(methodDeclarationSyntax, classRepresentation));
            }
        }
    }
}
