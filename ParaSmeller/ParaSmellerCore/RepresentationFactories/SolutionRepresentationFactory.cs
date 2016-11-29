using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ParaSmellerCore.Hierarchy;
using ParaSmellerCore.Representation;
using ParaSmellerCore.RepresentationFactories.ProgressInformation;
using ParaSmellerCore.SyntaxNodeUtils;
using ParaSmellerCore.TypeExtensions;

namespace ParaSmellerCore.RepresentationFactories
{
    public static class SolutionRepresentationFactory
    {

        public static  SolutionRepresentation Create(Compilation compilation)
        {
            
            Logger.Debug("Creating SolutionRepresentation");
            var solution = new SolutionRepresentation(compilation.AssemblyName.Trim());
            AddSyntaxTrees(solution, compilation);
            HierarchyLoader.Load(solution);
            ConnectInvocations(solution);
            return solution;
        }

        private static void ConnectInvocations(SolutionRepresentation solution)
        {
            Logger.Debug("ConnectInvocations");
            var invocations = solution.InvocationsToConnext();
            var counter = 0;
            var total = invocations.Count;
            
            Logger.Debug($"Total Invocations {total}");
            Parallel.ForEach(invocations, invocationExpressionRepresentation =>
            {
                ConnectInvocations(solution, invocationExpressionRepresentation, ref counter, total);
            });
        }

        private static void ConnectInvocations(SolutionRepresentation solution,
            InvocationExpressionRepresentation invocationExpressionRepresentation, ref int counter, int total)
        {
            var calledClassOriginal = invocationExpressionRepresentation.CalledClassOriginal;
            if (solution.Members.ContainsKey(invocationExpressionRepresentation.Defintion))
            {
                invocationExpressionRepresentation.InvokedImplementations.AddRange(
                    solution.Members[invocationExpressionRepresentation.Defintion]);
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
            if (counter%100 == 0)
            {
                Logger.Debug($"Current Invocation {counter} / {total}");
            }
            Interlocked.Increment(ref counter);
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


        private static void AddSyntaxTrees(SolutionRepresentation solution, Compilation compilation)
        {
            var scopeCalculator = new ScopeCalculator(compilation);
            var countClasses =  scopeCalculator.CountTypes();
            Logger.Debug($"Total SyntaxTrees: {scopeCalculator.CountSyntaxTrees()}");           
            Logger.Debug($"Total Classes & Interfaces: {countClasses}");

            var counter = 0;
            foreach (var syntaxTree in compilation.SyntaxTrees)
            {
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var classes =  SyntaxNodeFilter.GetClasses(syntaxTree);
                var interfaces = SyntaxNodeFilter.GetInterfaces(syntaxTree);
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
                solution.AddClass(classRepresentation);
                AnnounceTypeProgress(counter);
            }   
        }

        private static void AnnounceTypeProgress( int counter)
        {
            if (counter%10 == 0)
            {
                Logger.Debug("" + counter);
            }
        }


        private static void AddInterfaceRepresentations(SolutionRepresentation solution,
            IEnumerable<InterfaceDeclarationSyntax> interfaces, SemanticModel semanticModel, ref int counter)
        {
            foreach (var interfaceDeclarationSyntax in interfaces)
            {
                counter++;
                var interfaceRepresentation = InterfaceRepresentationFactory.Create(interfaceDeclarationSyntax,
                    semanticModel);
                solution.AddInterface(interfaceRepresentation);
                AnnounceTypeProgress(counter);
            }
        }
    }
}
