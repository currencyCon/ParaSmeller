using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace ConcurrencyChecker.NestedSynchronizedMethodClassChecker
{
    internal static class TypeExtension
    {
        public static IEnumerable<ITypeSymbol> GetBaseTypesAndThis(this ITypeSymbol type)
        {
            var current = type;
            while (current != null)
            {
                yield return current;
                current = current.BaseType;
            }
        }
    }

}    
