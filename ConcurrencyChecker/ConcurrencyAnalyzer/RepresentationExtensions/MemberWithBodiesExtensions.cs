﻿using System.Collections.Generic;
using System.Linq;
using ConcurrencyAnalyzer.Representation;
using ConcurrencyAnalyzer.SyntaxFilters;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConcurrencyAnalyzer.RepresentationExtensions
{
    public static class MemberWithBodiesExtensions
    {
        
        public static List<string> GetAllLockPossibilities(this IMember member)
        {
            var lockObjects = new List<string>();

            foreach (var block in member.Blocks)
            {
                if (block is LockBlock)
                {
                    lockObjects.Add(((LockStatementSyntax)block.Implementation).Expression.ToString());
                }
                GetLockArgument(block, lockObjects);
            }
            
            return lockObjects;
        }

        
        private static void GetLockArgument(IBody block, List<string> lockObjects)
        {
            if (block is LockBlock)
            {
                lockObjects.Add(((LockStatementSyntax) block.Implementation).Expression.ToString());
            }
            foreach (var subLockBlock in block.Blocks)
            {
                GetLockArgument(subLockBlock, lockObjects);
            }
        }

        public static IEnumerable<LockStatementSyntax> GetLockStatements(this IMember member)
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

        public static bool IsSynchronized(this IMember member)
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

        public static TChild GetFirstChild<TChild>(this IMember member)
        {
            return member.GetChildren<TChild>().FirstOrDefault();
        }

        public static IEnumerable<TChildren> GetChildren<TChildren>(this IMember member)
        {
            return member.Blocks.SelectMany(e => e.Implementation.GetChildren<TChildren>());
        }

        public static TParent GetFirstParent<TParent>(this IMember member)
        {
            var body = member.Blocks.FirstOrDefault();
            if (body?.Implementation != null)
            {
                return body.Implementation.GetFirstParent<TParent>();
            }
            return default(TParent);
        }

        public static IEnumerable<TParents> GetParents<TParents>(this IMember member)
        {
            var body = member.Blocks.FirstOrDefault();
            if (body?.Implementation != null)
            {
                return body.Implementation.GetParents<TParents>();
            }
            return new List<TParents>();
        }
    }
}