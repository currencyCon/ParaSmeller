using System.Collections.Generic;
using System.Linq;
using ConcurrencyAnalyzer.Representation;
using ConcurrencyAnalyzer.SyntaxNodeUtils;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConcurrencyAnalyzer.RepresentationExtensions
{
    public static class MemberExtensions
    {
        
        public static List<string> GetAllLockArguments(this Member member)
        {
            var lockObjects = new List<string>();

            foreach (var block in member.Blocks)
            {
                if (block is LockBlock)
                {
                    lockObjects.Add(((LockStatementSyntax)block.Implementation).Expression.ToString());
                }
                AppendLockArguments(block, lockObjects);
            }
            
            return lockObjects;
        }

        
        private static void AppendLockArguments(Body block, ICollection<string> lockObjects)
        {
            if (block is LockBlock)
            {
                lockObjects.Add(((LockStatementSyntax) block.Implementation).Expression.ToString());
            }
            foreach (var subLockBlock in block.Blocks)
            {
                AppendLockArguments(subLockBlock, lockObjects);
            }
        }

        public static IEnumerable<LockStatementSyntax> GetLockStatements(this Member member)
        {
            return GetLockStatements(member.Blocks);
        }

        public static List<LockStatementSyntax> GetLockStatements(IEnumerable<Body> bodies)
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

        public static IEnumerable<TChildren> GetChildren<TChildren>(this Member member)
        {
            return member.Blocks.SelectMany(e => e.Implementation.GetChildren<TChildren>());
        }
    }
}