using System.Collections.Generic;
using System.Linq;
using ConcurrencyAnalyzer.Representation;
using ConcurrencyAnalyzer.SyntaxFilters;
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
                if (body.IsSynchronized)
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
                if (body.IsSynchronized || IsSynchronized(body.Blocks))
                {
                    isSynchronized = true;
                }
            }
            return isSynchronized;
        }

        public static TChild GetFirstChild<TChild>(this IMemberWithBody memberWithBody)
        {
            return memberWithBody.GetChildren<TChild>().FirstOrDefault();
        }
        public static IEnumerable<TChildren> GetChildren<TChildren>(this IMemberWithBody memberWithBody)
        {
            return memberWithBody.Blocks.SelectMany(e => e.Implementation.GetChildren<TChildren>());
        }

        public static TParent GetFirstParent<TParent>(this IMemberWithBody memberWithBody)
        {
            var body = memberWithBody.Blocks.FirstOrDefault();
            if (body?.Implementation != null)
            {
                return body.Implementation.GetFirstParent<TParent>();
            }
            return default(TParent);
        }

        public static IEnumerable<TParents> GetParents<TParents>(this IMemberWithBody memberWithBody)
        {
            var body = memberWithBody.Blocks.FirstOrDefault();
            if (body?.Implementation != null)
            {
                return body.Implementation.GetParents<TParents>();
            }
            return new List<TParents>();
        }
    }
}