using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ConcurrencyAnalyzer.Representation;
using ConcurrencyAnalyzer.SymbolExtensions;
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
                var invocations = memberBlocks.SelectMany(e => e.GetAllInvocations()).ToList();
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

        public static void ConnectInvocations(SolutionRepresentation solution)
        {
            Logger.DebugLog("ConnectInvocations");
            var memberWithBodies = solution.Classes.SelectMany(e => e.Members).ToList();
            var memberBlocks = memberWithBodies.SelectMany(a => a.Blocks).ToList();
            var invocations = memberBlocks.SelectMany(e => e.GetAllInvocations()).Where(e => !e.InvokedImplementations.Any()).ToList();
            var counter = 0;
            var total = invocations.Count;
            LoadHierarchies(solution);
            Logger.DebugLog($"Total Invocations {total}");
            Parallel.ForEach(invocations, (invocationExpressionRepresentation) => 
            {
                if (solution.Members.ContainsKey(invocationExpressionRepresentation.Defintion))
                {
                    invocationExpressionRepresentation.InvokedImplementations.AddRange(
                        solution.Members[invocationExpressionRepresentation.Defintion]);
                    Logger.DebugLog($"Bukachu");

                }
                else
                {
                    foreach (var memberWithBody in memberWithBodies)
                    {
                        if (IsInvocatedTarget(invocationExpressionRepresentation, memberWithBody, solution))
                        {
                            invocationExpressionRepresentation.InvokedImplementations.Add(memberWithBody);
                        }
                    }
                }

                Logger.DebugLog($"Current Invocation {counter} / {total}");
                Interlocked.Increment(ref counter);
            });
/*            foreach (var invocationExpressionRepresentation in invocations)
            {
                if (solution.Members.ContainsKey(invocationExpressionRepresentation.Defintion))
                {
                    invocationExpressionRepresentation.InvokedImplementations.Add(
                        solution.Members[invocationExpressionRepresentation.Defintion]);
                }
                else
                {
                    foreach (var memberWithBody in memberWithBodies)
                    {
                        if (IsInvocatedTarget(invocationExpressionRepresentation, memberWithBody, solution))
                        {
                            invocationExpressionRepresentation.InvokedImplementations.Add(memberWithBody);
                        }
                    }
                }

                Logger.DebugLog($"Current Invocation {counter} / {total}");
                counter++;
            }*/
        }

        private static void LoadHierarchies(SolutionRepresentation solution)
        {
            Logger.DebugLog("LoadHierarchies");
            foreach (var clazz in solution.Classes)
            {
                var hierarchieChecker = new HierarchieChecker(clazz.NamedTypeSymbol);
                foreach (var baseClass in hierarchieChecker.InheritanceFromClass)
                {
                    var baseClassRepresentation = solution.GetClass(baseClass.OriginalDefinition.ToString());
                    if (baseClassRepresentation != null)
                    {
                        try
                        {
                            clazz.ClassMap.Add(baseClass.OriginalDefinition.ToString(), baseClassRepresentation);
                        }
                            catch (Exception)
                        {

                        }
                }
                }
                foreach (var interfacee in hierarchieChecker.InheritanceFromInterfaces)
                {
                    var interfaceRepresentation = solution.GetInterface(interfacee.OriginalDefinition.ToString());
                    if (interfaceRepresentation != null)
                    {
                        try
                        {
                            clazz.InterfaceMap.Add(interfacee.OriginalDefinition.ToString(), interfaceRepresentation);
                        }
                        catch (Exception)
                        {
                            
                        }
                    }
                }
            }
            Logger.DebugLog("LoadHierarchies finished");
        }

        private static bool IsInvocatedTarget(InvocationExpressionRepresentation invocationExpressionRepresentation, Member memberWithBody, SolutionRepresentation solution)
        {
            if (invocationExpressionRepresentation.Defintion == memberWithBody.OriginalDefinition)
            {
                return true;
            }
            //var hierarchieChecker = new HierarchieChecker(memberWithBody.ContainingClass.NamedTypeSymbol);
            
            if (CheckInheritatedClasses(invocationExpressionRepresentation, memberWithBody.ContainingClass))
            {
                return true;
            }

            if (CheckInheritatedInterfaces(invocationExpressionRepresentation, memberWithBody.ContainingClass))
            {
                return true;
            }

            return false;
        }

        private static bool CheckInheritatedInterfaces(InvocationExpressionRepresentation invocationExpressionRepresentation, ClassRepresentation clazz)
        {
            foreach (var interfacee in clazz.InterfaceMap.Values)
            {
                if (interfacee == null) continue;
                foreach (var member in clazz.Members)
                {
                    if (member.OriginalDefinition == invocationExpressionRepresentation.Defintion)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        
        private static bool CheckInheritatedClasses(InvocationExpressionRepresentation invocationExpressionRepresentation, ClassRepresentation clazz)
        {
            foreach(var baseClass in clazz.ClassMap.Values)
            {
                if (baseClass == null) continue;
                foreach (var member in baseClass.Members)
                {
                    if (member.OriginalDefinition == invocationExpressionRepresentation.Defintion)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static async Task AddSyntaxTrees(SolutionRepresentation solution, Compilation compilation)
        {
            var total = compilation.SyntaxTrees.ToList().Count;
            Logger.DebugLog("Total Compilations: " + total);

            var countClasses = await CountTypes(compilation);
            Logger.DebugLog("Total Classes & Interfaces: " + countClasses);

            var counter = 0;
            foreach (var syntaxTree in compilation.SyntaxTrees)
            {
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var classes = await SyntaxNodeFilter.GetClasses(syntaxTree);
                var interfaces = await SyntaxNodeFilter.GetInterfaces(syntaxTree);
                AddClassRepresentations(solution, classes, semanticModel, ref counter);
                AddInterfaceRepresentations(solution, interfaces, semanticModel, ref counter);
            }

            Logger.DebugLog("AddSyntaxTrees finished");
        }

        
        private static void AddClassRepresentations(SolutionRepresentation solution, IEnumerable<ClassDeclarationSyntax> classes, SemanticModel semanticModel, ref int counter)
        {   
            foreach (var classDeclarationSyntax in classes)
            {
                counter++;
                var classRepresentation = ClassRepresentationFactory.Create(classDeclarationSyntax, semanticModel);
                solution.Classes.Add(classRepresentation);
                try
                {
                    solution.ClassMap.Add(classRepresentation.NamedTypeSymbol.ToString(), classRepresentation);
                    foreach (var member in classRepresentation.Members.Distinct())
                    {
                        if (!solution.Members.ContainsKey(member.OriginalDefinition))
                        {
                            solution.Members.Add(member.OriginalDefinition, new List<Member>());
                        }
                        solution.Members[member.OriginalDefinition].Add(member);
                    }
                }
                catch (Exception)
                {
                    var name = classRepresentation.NamedTypeSymbol.ToString();
                    var test = 01;
                }
                if (counter%10 == 0)
                {
                    Logger.DebugLog(""+counter);
                }
            }   
        }

        private static void AddInterfaceRepresentations(SolutionRepresentation solution, IEnumerable<InterfaceDeclarationSyntax> interfaces, SemanticModel semanticModel, ref int counter)
        {
            foreach (var interfaceDeclarationSyntax in interfaces)
            {
                counter++;
                var interfaceRepresentation = InterfaceRepresentationFactory.Create(interfaceDeclarationSyntax, semanticModel);
                solution.Interfaces.Add(interfaceRepresentation);
                solution.InterfaceMap.Add(interfaceRepresentation.NamedTypeSymbol.ToString(), interfaceRepresentation);
                if (counter % 10 == 0)
                {
                    Logger.DebugLog("" + counter);
                }
            }
        }

        private static async Task<int> CountTypes(Compilation compilation)
        {
            var countClasses = 0;
            foreach (var syntaxTree in compilation.SyntaxTrees)
            {
                countClasses += await CountTypes(syntaxTree);
            }
            return countClasses;
        }

        private static async Task<int> CountTypes(SyntaxTree syntaxTree)
        {
            var classes = await SyntaxNodeFilter.GetClasses(syntaxTree);
            var interfaces = await SyntaxNodeFilter.GetInterfaces(syntaxTree);
            return classes.ToList().Count + interfaces.ToList().Count;
        }
    }
}
