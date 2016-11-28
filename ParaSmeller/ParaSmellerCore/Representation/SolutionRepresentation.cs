using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace ParaSmellerCore.Representation
{
    public class SolutionRepresentation
    {
        public readonly string Name;
        public readonly ConcurrentBag<ClassRepresentation> Classes = new ConcurrentBag<ClassRepresentation>();
        public readonly ConcurrentDictionary<string, ConcurrentBag<ClassRepresentation>> ClassMap = new ConcurrentDictionary<string, ConcurrentBag<ClassRepresentation>>();
        public readonly ConcurrentDictionary<string, InterfaceRepresentation> InterfaceMap = new ConcurrentDictionary<string, InterfaceRepresentation>();
        public readonly ConcurrentDictionary<string, ConcurrentBag<Member>> Members = new ConcurrentDictionary<string, ConcurrentBag<Member>>();
        public SolutionRepresentation(string name)
        {
            Name = name;
        }

        public ConcurrentBag<ClassRepresentation> GetClass(string className)
        {
            return GetType(ClassMap, className);
        }

        public InterfaceRepresentation GetInterface(string interfaceName)
        {
            return GetType(InterfaceMap, interfaceName);
        }

        private static TType GetType<TType>(IDictionary<string, TType> typeMap, string name)
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
                var memberList = Members.GetOrAdd(member.OriginalDefinition, new ConcurrentBag<Member>());
                memberList.Add(member);
            }
        }

        public void AddClass(ClassRepresentation classRepresentation)
        {
            Classes.Add(classRepresentation);
            var className = classRepresentation.NamedTypeSymbol.ToString();
            var classList = ClassMap.GetOrAdd(className, new ConcurrentBag<ClassRepresentation>());
            classList.Add(classRepresentation);
            AddMembers(classRepresentation);
        }

        public void AddInterface(InterfaceRepresentation interfaceRepresentation)
        {
            if (!InterfaceMap.TryAdd(interfaceRepresentation.NamedTypeSymbol.ToString(),
                interfaceRepresentation))
            {
                Logger.Debug($"Try to add Interface twice{interfaceRepresentation.NamedTypeSymbol}");
            }
        }

        public List<InvocationExpressionRepresentation> InvocationsToConnext()
        {
            var memberWithBodies = Classes.SelectMany(e => e.Members).ToList();
            var memberBlocks = memberWithBodies.SelectMany(a => a.Blocks).ToList();
            var invocations =
                memberBlocks.SelectMany(e => e.GetAllInvocations())
                    .Where(
                        e =>
                            !e.InvokedImplementations.Any() &&
                            !RepresentationFactories.AnalysisConfiguration.AnalysisConfiguration.NamesSpacesToExclude.Contains(e.TopLevelNameSpace))
                    .ToList();
            return invocations;
        }
    }
}
