using System.Collections.Generic;
using ConcurrencyAnalyzer.Representation;

namespace ConcurrencyAnalyzer.Hierarchy
{
    public static class HierarchyLoader
    {
        public static void Load(SolutionRepresentation solution)
        {
            Logger.Debug("HierarchyLoader");
            foreach (var clazz in solution.Classes)
            {
                var hierarchieChecker = new HierarchyChecker(clazz.NamedTypeSymbol);
                AddBaseClasses(solution, hierarchieChecker, clazz);
                AddInterfaces(solution, hierarchieChecker, clazz);
            }
            Logger.Debug("HierarchyLoader finished");
        }

        private static void AddInterfaces(SolutionRepresentation solution, HierarchyChecker hierarchyChecker, ClassRepresentation clazz)
        {
            foreach (var interfacee in hierarchyChecker.InheritanceFromInterfaces)
            {
                var interfaceRepresentation = solution.GetInterface(interfacee.OriginalDefinition.ToString());
                if (interfaceRepresentation != null)
                {
                    if (!clazz.InterfaceMap.ContainsKey(interfacee.OriginalDefinition.ToString()))
                    {
                        clazz.InterfaceMap.Add(interfacee.OriginalDefinition.ToString(), interfaceRepresentation);
                    }

                    interfaceRepresentation.ImplementingClasses.Add(clazz);
                }
            }
        }

        private static void AddBaseClasses(SolutionRepresentation solution, HierarchyChecker hierarchyChecker, ClassRepresentation clazz)
        {
            foreach (var baseClass in hierarchyChecker.InheritanceFromClass)
            {
                var baseClassRepresentations = solution.GetClass(baseClass.OriginalDefinition.ToString());
                if (baseClassRepresentations != null)
                {
                    foreach (var baseClassRepresentation in baseClassRepresentations)
                    {
                        if (!clazz.ClassMap.ContainsKey(baseClass.OriginalDefinition.ToString()))
                        {
                            clazz.ClassMap.Add(baseClass.OriginalDefinition.ToString(), new List<ClassRepresentation>());
                        }
                        clazz.ClassMap[baseClass.OriginalDefinition.ToString()].Add(baseClassRepresentation);
                    }
                }
            }
        }
    }
}