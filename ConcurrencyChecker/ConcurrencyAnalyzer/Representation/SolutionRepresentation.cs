using System.Collections.Generic;

namespace ConcurrencyAnalyzer.Representation
{
    public class SolutionRepresentation
    {
        public readonly string Name;
        public readonly ICollection<ClassRepresentation> Classes;
        public readonly ICollection<InterfaceRepresentation> Interfaces;
        public readonly Dictionary<string, ClassRepresentation> ClassMap;
        public readonly Dictionary<string, InterfaceRepresentation> InterfaceMap;
        public readonly Dictionary<string, ICollection<Member>> Members;
        public SolutionRepresentation(string name)
        {
            Name = name;
            Classes = new List<ClassRepresentation>();
            Interfaces = new List<InterfaceRepresentation>();
            ClassMap = new Dictionary<string, ClassRepresentation>();
            InterfaceMap = new Dictionary<string, InterfaceRepresentation>();
            Members = new Dictionary<string, ICollection<Member>>();
        }

        public ClassRepresentation GetClass(string className)
        {
            return GetType(ClassMap, className);
        }

        public InterfaceRepresentation GetInterface(string interfaceName)
        {
            return GetType(InterfaceMap, interfaceName);
        }

        private static TType GetType<TType>(IReadOnlyDictionary<string, TType> typeMap, string name)
        {
            TType type;
            typeMap.TryGetValue(name, out type);
            return type;
        }
    }
}
