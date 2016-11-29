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

        private static void AddInterfaces(SolutionRepresentation solution, Hierarchy hierarchy, ClassRepresentation clazz)
        {
            foreach (var interfacee in hierarchy.InheritanceFromInterfaces)
            {
                var interfaceRepresentation = solution.GetInterface(interfacee.OriginalDefinition.ToString());
                if (interfaceRepresentation != null)
                {
                    clazz.AddInterface(interfacee.OriginalDefinition.ToString(), interfaceRepresentation);
                    interfaceRepresentation.ImplementingClasses.Add(clazz);
                }
            }
        }

        private static void AddBaseClasses(SolutionRepresentation solution, Hierarchy hierarchy, ClassRepresentation clazz)
        {
            foreach (var baseClass in hierarchy.InheritanceFromClass)
            {
                AddClassesInHierarchy(solution, clazz, baseClass.OriginalDefinition.ToString());
            }
        }

        private static void AddClassesInHierarchy(SolutionRepresentation solution, ClassRepresentation clazz,
            string baseClassName)
        {
            var baseClassRepresentations = solution.GetClass(baseClassName);
            if (baseClassRepresentations != null)
            {
                AddClassesInHierarchy(clazz, baseClassRepresentations, baseClassName);
            }
        }

        private static void AddClassesInHierarchy(ClassRepresentation clazz, IEnumerable<ClassRepresentation> baseClassRepresentations,
            string baseClassName)
        {
            foreach (var baseClassRepresentation in baseClassRepresentations)
            {
                clazz.AddClassInHierarchy(baseClassName, baseClassRepresentation);
            }
        }
    }
}