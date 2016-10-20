using System.Collections.Generic;
using System.Linq;
using ConcurrencyAnalyzer.Representation;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
        
        public static List<IMemberWithBody> GetMembersWithMultipleLocks(this ClassRepresentation classRepresentation)
        {
            List<IMemberWithBody> members = new List<IMemberWithBody>();
            int counter = 0;
            foreach (var memberWithBody in classRepresentation.Members)
            {
                foreach (var block in memberWithBody.Blocks)
                {
                    if (block is LockBlock)
                    {
                        counter++;
                    }
                    GetNextDepthLock(block, members, counter, memberWithBody);
                }
            }

            return members;
        }

        private static void GetNextDepthLock(IBody block, List<IMemberWithBody> members, int counter, IMemberWithBody memberWithBody)
        {
            if (counter == 2 && !members.Contains(memberWithBody))
            {
                members.Add(memberWithBody);
            }

            foreach (var subBlock in block.Blocks)
            {
                if (subBlock is LockBlock)
                {
                    counter++;
                }
                GetNextDepthLock(subBlock, members, counter, memberWithBody);
            }
        }




    }
}