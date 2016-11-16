using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
            int counter = 0;
            var total = invocations.Count;
            Logger.DebugLog($"Total Invocations {total}");
            foreach (var invocationExpressionRepresentation in invocations)
            {
                foreach (var memberWithBody in memberWithBodies)
                {
                    if (IsInvocatedTarget(invocationExpressionRepresentation, memberWithBody, solution))
                    {
                        invocationExpressionRepresentation.InvokedImplementations.Add(memberWithBody);
                    }
                }

                if (invocationExpressionRepresentation.InvokedImplementations.Count == 0)
                {
                    //Logger.DebugLog($"ERROR: Wrong Matching of impl:{invocationExpressionRepresentation.Implementation.ToString()}");
                    //throw new NotImplementedException($"Wrong Matching of impl:{invocationExpressionRepresentation.Implementation.ToString()}");
                }
                Logger.DebugLog($"Current Invocation {counter} / {total}");
                counter++;
            }
        }

        private static bool IsInvocatedTarget(InvocationExpressionRepresentation invocationExpressionRepresentation, Member memberWithBody, SolutionRepresentation solution)
        {
            if (invocationExpressionRepresentation.Defintion == memberWithBody.OriginalDefinition)
            {
                return true;
            }
            var semanticModel = memberWithBody.ContainingClass.SemanticModel;
            var classTypeSymbol = semanticModel.GetDeclaredSymbol(memberWithBody.ContainingClass.Implementation) as INamedTypeSymbol;
            var hierarchieChecker = new HierarchieChecker(classTypeSymbol);
            
            if (CheckInheritatedClasses(invocationExpressionRepresentation, solution, hierarchieChecker))
            {
                return true;
            }

            if (CheckInheritatedInterfaces(invocationExpressionRepresentation, solution, hierarchieChecker))
            {
                return true;
            }

            return false;
        }

        private static bool CheckInheritatedInterfaces(InvocationExpressionRepresentation invocationExpressionRepresentation, SolutionRepresentation solution, HierarchieChecker hierarchieChecker)
        {
            foreach (var upperInterface in hierarchieChecker.InheritanceFromInterfaces)
            {
                var interfaceName = upperInterface.ToString();
                var clazz = GetInterface(interfaceName, solution);
                if (clazz != null)
                {
                    foreach (var member in clazz.Members)
                    {
                        if (member.OriginalDefinition == invocationExpressionRepresentation.Defintion)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        
        private static bool CheckInheritatedClasses(InvocationExpressionRepresentation invocationExpressionRepresentation, SolutionRepresentation solution, HierarchieChecker hierarchieChecker)
        {
            foreach (var upperClass in hierarchieChecker.InheritanceFromClass)
            {
                var className = upperClass.ToString();
                var clazz = GetClass(className, solution);
                if (clazz != null)
                {
                    foreach (var member in clazz.Members)
                    {
                        if (member.OriginalDefinition == invocationExpressionRepresentation.Defintion)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        
        private static InterfaceRepresentation GetInterface(string interfaceName, SolutionRepresentation solution)
        {
            InterfaceRepresentation interfacee = null;
            solution.InterfaceMap.TryGetValue(interfaceName, out interfacee);
            return interfacee;
        }
        
        private static ClassRepresentation GetClass(string className, SolutionRepresentation solution)
        {
            ClassRepresentation clazz = null;
            solution.ClassMap.TryGetValue(className, out clazz);
            return clazz;
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

            int countClasses = await CountClassAndInterface(compilation); ;
            Logger.DebugLog("Total Classes & Interfaces: " + countClasses);

            int counter = 0;
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
                solution.ClassMap.Add(classRepresentation.NamedTypeSymbol.ToString(), classRepresentation);
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

        private static async Task<int> CountClassAndInterface(Compilation compilation)
        {
            int countClasses = 0;
            foreach (var syntaxTree in compilation.SyntaxTrees)
            {
                var classes = await SyntaxNodeFilter.GetClasses(syntaxTree);
                var interfaces = await SyntaxNodeFilter.GetInterfaces(syntaxTree);
                countClasses += classes.ToList().Count + interfaces.ToList().Count;
            }
            return countClasses;
        }

    }
}
