using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ConcurrencyAnalyzer.Hierarchy;
using ConcurrencyAnalyzer.Representation;
using ConcurrencyAnalyzer.RepresentationFactories.ProgressInformation;
using ConcurrencyAnalyzer.SyntaxNodeUtils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
namespace ConcurrencyAnalyzer.RepresentationFactories
{
    public static class SolutionRepresentationFactory
    {

        public static async Task<SolutionRepresentation> Create(Compilation compilation)
        {
            
            Logger.Debug("Creating SolutionRepresentation");
            var solution = new SolutionRepresentation(compilation.AssemblyName.Trim());
            await AddSyntaxTrees(solution, compilation);
            HierarchyLoader.Load(solution);
            ConnectInvocations(solution);
            return solution;
        }

        private static void ConnectInvocations(SolutionRepresentation solution)
        {
            Logger.Debug("ConnectInvocations");
            var memberWithBodies = solution.Classes.SelectMany(e => e.Members).ToList();
            var memberBlocks = memberWithBodies.SelectMany(a => a.Blocks).ToList();
            var invocations = memberBlocks.SelectMany(e => e.GetAllInvocations()).Where(e => !e.InvokedImplementations.Any() && !AnalysisConfiguration.AnalysisConfiguration.NamesSpacesToExclude.Contains(e.TopLevelNameSpace)).ToList();
            var counter = 0;
            var total = invocations.Count;
            
            Logger.Debug($"Total Invocations {total}");
            Parallel.ForEach(invocations, invocationExpressionRepresentation =>
            {
                var calledClassOriginal = invocationExpressionRepresentation.CalledClassOriginal;
                if (solution.Members.ContainsKey(invocationExpressionRepresentation.Defintion))
                {
                    invocationExpressionRepresentation.InvokedImplementations.AddRange(solution.Members[invocationExpressionRepresentation.Defintion]);
                }
                else if (solution.ClassMap.ContainsKey(calledClassOriginal))
                {
                    foreach (var member in solution.ClassMembers(calledClassOriginal))
                    {
                        AddAsImplementationIfTarget(invocationExpressionRepresentation, member);
                    }
                }
                else if (solution.InterfaceMap.ContainsKey(calledClassOriginal))
                {
                    foreach (var member in solution.ImplementedInterfaceMembers(calledClassOriginal))
                    {
                        AddAsImplementationIfTarget(invocationExpressionRepresentation, member);
                    }
                }
                if (counter % 100 == 0)
                {
                    Logger.Debug($"Current Invocation {counter} / {total}");
                }
                Interlocked.Increment(ref counter);
            });
        }

        private static void AddAsImplementationIfTarget(InvocationExpressionRepresentation invocationExpressionRepresentation, Member member)
        {
            if (IsInvocatedTarget(invocationExpressionRepresentation, member))
            {
                invocationExpressionRepresentation.InvokedImplementations.Add(member);
            }
        }

        private static bool IsInvocatedTarget(InvocationExpressionRepresentation invocationExpressionRepresentation, Member memberWithBody)
        {
            if (invocationExpressionRepresentation.Defintion == memberWithBody.OriginalDefinition)
            {
                return true;
            }
            
            if (memberWithBody.ContainingClass.ImplementsInvocationDefinedInBaseClasses(invocationExpressionRepresentation))
            {
                return true;
            }

            if (memberWithBody.ContainingClass.ImplementsInvocationDefinedInInterfaces(invocationExpressionRepresentation))
            {
                return true;
            }

            return false;
        }


        private static async Task AddSyntaxTrees(SolutionRepresentation solution, Compilation compilation)
        {
            ScopeCalculator scopeCalculator = new ScopeCalculator(compilation);

            var countClasses = await scopeCalculator.CountTypes();

            Logger.Debug($"Total SyntaxTrees: {scopeCalculator.CountSyntaxTrees()}");           
            Logger.Debug($"Total Classes & Interfaces: {countClasses}");

            var counter = 0;
            foreach (var syntaxTree in compilation.SyntaxTrees)
            {
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var classes = await SyntaxNodeFilter.GetClasses(syntaxTree);
                var interfaces = await SyntaxNodeFilter.GetInterfaces(syntaxTree);
                AddClassRepresentations(solution, classes, semanticModel, ref counter);
                AddInterfaceRepresentations(solution, interfaces, semanticModel, ref counter);
            }

            Logger.Debug("AddSyntaxTrees finished");
        }

        
        private static void AddClassRepresentations(SolutionRepresentation solution, IEnumerable<ClassDeclarationSyntax> classes, SemanticModel semanticModel, ref int counter)
        {   
            foreach (var classDeclarationSyntax in classes)
            {
                counter++;
                var classRepresentation = ClassRepresentationFactory.Create(classDeclarationSyntax, semanticModel);
                solution.Classes.Add(classRepresentation);

                var className = classRepresentation.NamedTypeSymbol.ToString();
                if (!solution.ClassMap.ContainsKey(className))
                {
                    solution.ClassMap.Add(className, new List<ClassRepresentation>());
                }
                solution.ClassMap[className].Add(classRepresentation);
                solution.AddMembers(classRepresentation);
                
                if (counter%10 == 0)
                {
                    Logger.Debug(""+counter);
                }
            }   
        }
        

        private static void AddInterfaceRepresentations(SolutionRepresentation solution, IEnumerable<InterfaceDeclarationSyntax> interfaces, SemanticModel semanticModel, ref int counter)
        {
            foreach (var interfaceDeclarationSyntax in interfaces)
            {
                counter++;
                var interfaceRepresentation = InterfaceRepresentationFactory.Create(interfaceDeclarationSyntax, semanticModel);
                solution.InterfaceMap.Add(interfaceRepresentation.NamedTypeSymbol.ToString(), interfaceRepresentation);
                if (counter % 10 == 0)
                {
                    Logger.Debug("" + counter);
                }
            }
        }
    }
}
