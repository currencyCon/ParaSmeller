using System.Collections.Generic;
using ConcurrencyAnalyzer.Representation;
using ConcurrencyAnalyzer.SyntaxFilters;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConcurrencyAnalyzer.RepresentationFactories
{
    public class SolutionRepresentationFactory
    {
        public static SolutionRepresentation Create(Compilation compilation)
        {
            var solution = new SolutionRepresentation(compilation.AssemblyName);
            AddClassRepresentations(solution, compilation.SyntaxTrees);
            return solution;
        }

        private static async void AddClassRepresentations(SolutionRepresentation solution, IEnumerable<SyntaxTree> syntaxTrees)
        {
            foreach (var syntaxTree in syntaxTrees)
            {
                var classes = await SyntaxNodeFilter.GetClasses(syntaxTree);
                AddClassRepresentation(solution, classes);
            }
        }

        private static void AddClassRepresentation(SolutionRepresentation solution, IEnumerable<ClassDeclarationSyntax> classes)
        {
            foreach (var classDeclarationSyntax in classes)
            {
                solution.Classes.Add(ClassRepresentationFactory.Create(classDeclarationSyntax));
            }
        }
    }
}
