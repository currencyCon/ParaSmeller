using System.Collections.Generic;
using System.Linq;
using ConcurrencyAnalyzer.Representation;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConcurrencyAnalyzer.RepresentationExtensions
{
    public static class MemberWithBodiesExtensions
    {
        public static IEnumerable<LockStatementSyntax> GetLockStatements(this IMemberWithBody member)
        {
            return member.Blocks.Where(e => e is LockBlock).Select(a => a.Implementation as LockStatementSyntax);
        }

        public static bool IsSynchronized(this IMemberWithBody member)
        {
            return IsSynchronized(member.Blocks);
        }

        private static bool IsSynchronized(IEnumerable<IBody> bodies)
        {
            var isSynchronized = false;
            foreach (var body in bodies)
            {
                if (IsSynchronized(body) || IsSynchronized(body.Blocks))
                {
                    isSynchronized = true;
                }
            }
            return isSynchronized;
        }

        private static bool IsSynchronized(IBody body)
        {
            return body is LockBlock;
        }
    }
}