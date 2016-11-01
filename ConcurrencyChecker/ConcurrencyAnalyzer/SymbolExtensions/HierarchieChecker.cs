using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace ConcurrencyAnalyzer.SymbolExtensions
{
    public class HierarchieChecker
    {
        readonly List<string> _inheritanceClasses = new List<string>();

        public HierarchieChecker(ITypeSymbol type)
        {
            var baseTypes = type.GetBaseTypesAndThis();
            var interfaces = type.AllInterfaces;
            foreach (var interfacee in interfaces)
            {
                _inheritanceClasses.Add(interfacee.Name);
            }
            foreach (var baseType in baseTypes)
            {
                _inheritanceClasses.Add(baseType.Name);
            }
        }

        public bool IsSubClass(ITypeSymbol baseType)
        {
            return _inheritanceClasses.Contains(baseType.Name);
        }
    }
}
