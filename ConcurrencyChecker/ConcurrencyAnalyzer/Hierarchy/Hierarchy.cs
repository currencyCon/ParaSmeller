using System.Collections.Generic;
using ConcurrencyAnalyzer.SymbolExtensions;
using Microsoft.CodeAnalysis;

namespace ConcurrencyAnalyzer.Hierarchy
{
    public class Hierarchy
    {
        private readonly List<string> _inheritanceClasses = new List<string>();
        public  readonly List<INamedTypeSymbol> InheritanceFromInterfaces = new List<INamedTypeSymbol>();
        public readonly List<ITypeSymbol> InheritanceFromClass = new List<ITypeSymbol>();

        public Hierarchy(ITypeSymbol type)
        {
            var baseTypes = type.GetBaseTypesAndThis();
            var interfaces = type.AllInterfaces;

            foreach (var interfacee in interfaces)
            {
                _inheritanceClasses.Add(interfacee.Name);
                InheritanceFromInterfaces.Add(interfacee);
            }
            foreach (var baseType in baseTypes)
            {
                _inheritanceClasses.Add(baseType.Name);
                InheritanceFromClass.Add(baseType);
            }
            RemoveSelfFromInheritanceTree();
        }

        private void RemoveSelfFromInheritanceTree()
        {
            InheritanceFromClass.RemoveAt(0);
        }

        public bool IsSubClass(ITypeSymbol baseType)
        {
            return _inheritanceClasses.Contains(baseType.Name);
        }
    }
}
