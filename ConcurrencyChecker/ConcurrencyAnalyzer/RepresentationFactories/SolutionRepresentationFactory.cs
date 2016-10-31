using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConcurrencyAnalyzer.Representation;
using ConcurrencyAnalyzer.SyntaxFilters;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConcurrencyAnalyzer.RepresentationFactories
{
    public static class SolutionRepresentationFactory
    {
        public static async Task<SolutionRepresentation> Create(Compilation compilation)
        {
            var solution = new SolutionRepresentation(compilation.AssemblyName.Trim());
            await AddSyntaxTrees(solution, compilation);
            ConnectInvocations(solution);
            ConnectReverseInvocations(solution);
            return solution;
        }

        private static void ConnectReverseInvocations(SolutionRepresentation solution)
        {
            foreach (var clazz in solution.Classes)
            {
                var memberWithBodies = clazz.Members;
                var memberBlocks = memberWithBodies.SelectMany(a => a.Blocks).ToList();
                var invocations = memberBlocks.SelectMany(GetInvocations).ToList();
                foreach (var invocationExpressionRepresentation in invocations)
                { 
                    foreach (var memberWithBody in memberWithBodies)
                    {
                        if (invocationExpressionRepresentation.InvocationTargetName.ToString() ==
                            memberWithBody.Name.ToString())
                        {
                            memberWithBody.Callers.Add(invocationExpressionRepresentation);
                        }
                    }
                }
            }
        }

        private static void ConnectInvocations(SolutionRepresentation solution)
        {
            var memberWithBodies = solution.Classes.SelectMany(e => e.Members).ToList();
            var memberBlocks = memberWithBodies.SelectMany(a => a.Blocks).ToList();
            var invocations = memberBlocks.SelectMany(GetInvocations).ToList();
            foreach (var invocationExpressionRepresentation in invocations)
            {
                invocationExpressionRepresentation.InvokedImplementation =
                    memberWithBodies.FirstOrDefault(
                        e =>
                            e.ContainingClass.Name.ToString() == invocationExpressionRepresentation.CalledClass&&
                            e.Name.ToString() == invocationExpressionRepresentation.InvocationTargetName.ToString()
                            );
            }
        }

        private static IEnumerable<InvocationExpressionRepresentation> GetInvocations(IBody body)
        {
            IEnumerable<InvocationExpressionRepresentation> invocations = new List<InvocationExpressionRepresentation>();
            invocations = invocations.Concat(body.InvocationExpressions);
            foreach (var block in body.Blocks)
            {
                invocations = invocations.Concat(GetInvocations(block));
            }
            return invocations;
        }

        private static async Task AddSyntaxTrees(SolutionRepresentation solution, Compilation compilation)
        {
            foreach (var syntaxTree in compilation.SyntaxTrees)
            {
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var classes = await SyntaxNodeFilter.GetClasses(syntaxTree);
                AddClassRepresentations(solution, classes, semanticModel);
            }
        }

        private static void AddClassRepresentations(SolutionRepresentation solution, IEnumerable<ClassDeclarationSyntax> classes, SemanticModel semanticModel)
        {
            foreach (var classDeclarationSyntax in classes)
            {
                solution.Classes.Add(ClassRepresentationFactory.Create(classDeclarationSyntax, semanticModel));
            }
        }
    }
}
