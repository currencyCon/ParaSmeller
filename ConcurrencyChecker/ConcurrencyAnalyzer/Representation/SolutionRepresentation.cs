using System.Collections.Generic;
using System.Linq;

namespace ConcurrencyAnalyzer.Representation
{
    public class SolutionRepresentation
    {
        public readonly string Name;
        public readonly ICollection<ClassRepresentation> Classes;
        public readonly ICollection<InterfaceRepresentation> Interfaces;
        public readonly Dictionary<string, ICollection<ClassRepresentation>> ClassMap;
        public readonly Dictionary<string, InterfaceRepresentation> InterfaceMap;
        public readonly Dictionary<string, ICollection<Member>> Members;
        public SolutionRepresentation(string name)
        {
            Name = name;
            Classes = new List<ClassRepresentation>();
            Interfaces = new List<InterfaceRepresentation>();
            ClassMap = new Dictionary<string, ICollection<ClassRepresentation>>();
            InterfaceMap = new Dictionary<string, InterfaceRepresentation>();
            Members = new Dictionary<string, ICollection<Member>>();
        }

        public ICollection<ClassRepresentation> GetClass(string className)
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

        public IEnumerable<Member> ImplementedInterfaceMembers(string className)
        {
            return InterfaceMap[className].ImplementingClasses.SelectMany(e => e.Members);
        }

        public IEnumerable<Member> ClassMembers(string className)
        {
            return ClassMap[className].SelectMany(e => e.Members);
        }

        public void AddMembers(ClassRepresentation classRepresentation)
        {
            foreach (var member in classRepresentation.Members.Distinct())
            {
                if (!Members.ContainsKey(member.OriginalDefinition))
                {
                    Members.Add(member.OriginalDefinition, new List<Member>());
                }
                Members[member.OriginalDefinition].Add(member);
            }
        }
    }
}
