using System.Linq;
using ConcurrencyAnalyzer.Representation;

namespace ConcurrencyAnalyzer.RepresentationExtensions
{
    public static class ClassRepresentationExtensions
    {
        public static bool ClassHasSynchronizedMember(this ClassRepresentation classRepresentation)
        {
            return classRepresentation.Members.Any(e => e.IsSynchronized());
        }

        public static IMemberWithBody GetMemberByName(this ClassRepresentation classRepresentation, string memberName)
        {
            return classRepresentation.Members.FirstOrDefault(e => e.Name.ToString() == memberName);
        }
    }
}