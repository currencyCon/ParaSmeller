using System.Collections.Generic;

namespace ConcurrencyAnalyzer.Representation
{
    public class SolutionRepresentation
    {
        public readonly string Name;
        public readonly ICollection<ClassRepresentation> Classes;
        public SolutionRepresentation(string name)
        {
            Name = name;
            Classes = new List<ClassRepresentation>();
        }
    }
}
