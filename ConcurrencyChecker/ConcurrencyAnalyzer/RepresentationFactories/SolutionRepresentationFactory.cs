using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ConcurrencyAnalyzer.Representation;
using ConcurrencyAnalyzer.SyntaxNodeUtils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConcurrencyAnalyzer.RepresentationFactories
{
    public static class SolutionRepresentationFactory
    {
        public static async Task<SolutionRepresentation> Create(Compilation compilation)
        {
            Logger.DebugLog("Creating SolutionRepresentation");
            var solution = new SolutionRepresentation(compilation.AssemblyName.Trim());
            await AddSyntaxTrees(solution, compilation);
            ConnectInvocations(solution);
            ConnectReverseInvocations(solution);
            return solution;
        }

        private static void ConnectReverseInvocations(SolutionRepresentation solution)
        {
            Logger.DebugLog("ConnectReverseInvocations");
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
            Logger.DebugLog("ConnectInvocations");
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
                if (invocationExpressionRepresentation.InvokedImplementation == null)
                {
                    throw new NotImplementedException($"Wrong Matching of impl:{invocationExpressionRepresentation.Implementation.ToString()}");
                }
            }
        }

        private static IEnumerable<InvocationExpressionRepresentation> GetInvocations(Body body)
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
            int total = compilation.SyntaxTrees.ToList().Count;
            Logger.DebugLog("Total Compilations: " + total);

            int countClasses = await CountClasses(compilation); ;
            Logger.DebugLog("Total Classes: " + countClasses);

            int counter = 0;
            foreach (var syntaxTree in compilation.SyntaxTrees)
            {
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var classes = await SyntaxNodeFilter.GetClasses(syntaxTree);
                AddClassRepresentations(solution, classes, semanticModel, ref counter);
            }

            Logger.DebugLog("AddSyntaxTrees finished");
        }

        
        private static void AddClassRepresentations(SolutionRepresentation solution, IEnumerable<ClassDeclarationSyntax> classes, SemanticModel semanticModel, ref int counter)
        {
            
            foreach (var classDeclarationSyntax in classes)
            {
                counter++;
                solution.Classes.Add(ClassRepresentationFactory.Create(classDeclarationSyntax, semanticModel));
                if (counter%10 == 0)
                {
                    Logger.DebugLog(""+counter);
                }
            }
            
        }

        private static async Task<int> CountClasses(Compilation compilation)
        {
            int countClasses = 0;
            foreach (var syntaxTree in compilation.SyntaxTrees)
            {
                var classes = await SyntaxNodeFilter.GetClasses(syntaxTree);
                countClasses += classes.ToList().Count;
            }
            return countClasses;
        }

    }
}
