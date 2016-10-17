using System.Collections.Generic;
using ConcurrencyAnalyzer.Representation;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConcurrencyAnalyzer.RepresentationExtensions
{
    public static class MemberWithBodiesExtensions
    {
        public static IEnumerable<LockStatementSyntax> GetLockStatements(this IMemberWithBody member)
        {
            return GetLockStatements(member.Blocks);
        }

        public static List<LockStatementSyntax> GetLockStatements(IEnumerable<IBody> bodies)
        {
            var lockStatementSyntaxs = new List<LockStatementSyntax>();
            foreach (var body in bodies)
            {
                if (body is LockBlock)
                {
                    lockStatementSyntaxs.Add(body.Implementation as LockStatementSyntax);
                }
                lockStatementSyntaxs.AddRange(GetLockStatements(body.Blocks));
            }
            return lockStatementSyntaxs;
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