using System.Collections.Generic;
using System.Linq;
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
            var solution = new SolutionRepresentation(compilation.AssemblyName.Trim());
            AddClassRepresentations(solution, compilation);
            ConnectInvocations(solution);
            return solution;
        }

        private static void ConnectInvocations(SolutionRepresentation solution)
        {
            var methods = solution.Classes.SelectMany(e => e.Methods).ToList();
            var invocations = methods.SelectMany(e => e.InvocationExpressions);
            foreach (var invocationExpressionRepresentation in invocations)
            {
                invocationExpressionRepresentation.MethodImplementation =
                    methods.FirstOrDefault(
                        e =>
                            e.MethodImplementation.Identifier.ToString() ==
                            invocationExpressionRepresentation.MethodName.ToString());
            }
        }

        private static async void AddClassRepresentations(SolutionRepresentation solution, Compilation compilation)
        {
            foreach (var syntaxTree in compilation.SyntaxTrees)
            {
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var classes = await SyntaxNodeFilter.GetClasses(syntaxTree);
                AddClassRepresentation(solution, classes, semanticModel);
            }
        }

        private static void AddClassRepresentation(SolutionRepresentation solution, IEnumerable<ClassDeclarationSyntax> classes, SemanticModel semanticModel)
        {
            foreach (var classDeclarationSyntax in classes)
            {
                solution.Classes.Add(ClassRepresentationFactory.Create(classDeclarationSyntax, semanticModel));
            }
        }
    }
}
