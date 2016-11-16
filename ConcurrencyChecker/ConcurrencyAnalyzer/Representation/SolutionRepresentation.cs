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

        public SolutionRepresentation(string name)
        {
            Name = name;
            Classes = new List<ClassRepresentation>();
            Interfaces = new List<InterfaceRepresentation>();
            ClassMap = new Dictionary<string, ClassRepresentation>();
            InterfaceMap = new Dictionary<string, InterfaceRepresentation>();
        }
    }
}
