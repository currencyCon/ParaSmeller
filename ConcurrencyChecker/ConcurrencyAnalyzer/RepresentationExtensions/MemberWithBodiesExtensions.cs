
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
    }
}
