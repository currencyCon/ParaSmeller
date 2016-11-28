using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using ParaSmellerCore.Representation;

namespace ParaSmellerCore.Hierarchy
{
    public static class HierarchyLoader
    {
        public static void Load(SolutionRepresentation solution)
        {
            Logger.Debug("HierarchyLoader startet");
            Parallel.ForEach(solution.Classes, clazz =>
            {
                var hierarchy = new Hierarchy(clazz.NamedTypeSymbol);
                AddBaseClasses(solution, hierarchy, clazz);
                AddInterfaces(solution, hierarchy, clazz);
            });
            Logger.Debug("HierarchyLoader finished");
        }

        private static void AddInterfaces(SolutionRepresentation solution, ParaSmellerCore.Hierarchy.Hierarchy hierarchy, ClassRepresentation clazz)
        {
            foreach (var interfacee in hierarchy.InheritanceFromInterfaces)
            {
                var interfaceRepresentation = solution.GetInterface(interfacee.OriginalDefinition.ToString());
                if (interfaceRepresentation != null)
                {
                    if (!clazz.InterfaceMap.TryAdd(interfacee.OriginalDefinition.ToString(), interfaceRepresentation))
                    {
                        Logger.Debug($"Tried to add Interface twice: {interfacee.OriginalDefinition}");
                    }

                    interfaceRepresentation.ImplementingClasses.Add(clazz);
                }
            }
        }

        private static void AddBaseClasses(SolutionRepresentation solution, ParaSmellerCore.Hierarchy.Hierarchy hierarchy, ClassRepresentation clazz)
        {
            foreach (var baseClass in hierarchy.InheritanceFromClass)
            {
                var baseClassRepresentations = solution.GetClass(baseClass.OriginalDefinition.ToString());
                if (baseClassRepresentations != null)
                {
                    foreach (var baseClassRepresentation in baseClassRepresentations)
                    {
                        var classList = clazz.ClassMap.GetOrAdd(baseClass.OriginalDefinition.ToString(), new ConcurrentBag<ClassRepresentation>());
                        classList.Add(baseClassRepresentation);
                    }
                }
            }
        }
    }
}