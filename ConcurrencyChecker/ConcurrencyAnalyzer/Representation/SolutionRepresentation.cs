using System.Collections.Generic;

namespace ConcurrencyAnalyzer.Representation
{
    public class SolutionRepresentation
    {
        public string Name { get; set; }
        public ICollection<ClassRepresentation> Classes { get; set; }
        public SolutionRepresentation(string name)
        {
            Name = name;
            Classes = new List<ClassRepresentation>();
        }
    }
}
