using System.Collections.Generic;
using System.Linq;
using ConcurrencyAnalyzer.Builders;
using ConcurrencyAnalyzer.RepresentationExtensions;
using ConcurrencyAnalyzer.SyntaxFilters;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConcurrencyAnalyzer.Representation
{
    public class ClassRepresentation
    {
        public readonly ClassDeclarationSyntax ClassDeclarationSyntax;
        public readonly string FullyQualifiedDomainName;
        public readonly SyntaxToken Name;
        public readonly ICollection<IMember> Members;

        private ICollection<MethodRepresentation> _synchronizedMethods;

        public ICollection<MethodRepresentation> SynchronizedMethods
        {
            get
            {
                return _synchronizedMethods ??
                       (_synchronizedMethods = Members.Where(e => e is MethodRepresentation && e.IsFullySynchronized())
                           .Select(e => e as MethodRepresentation).ToList());
            }
        }

        private ICollection<MethodRepresentation> _unSynchronizedMethods;

        public ICollection<MethodRepresentation> UnSynchronizedMethods
        {
            get
            {
                return _unSynchronizedMethods ??
                       (_unSynchronizedMethods = Members.Where(e => e is MethodRepresentation && !e.IsFullySynchronized())
                           .Select(e => e as MethodRepresentation).ToList());
            }
        }




        private ICollection<PropertyRepresentation> _synchronizedProperties;

        public ICollection<PropertyRepresentation> SynchronizedProperties
        {
            get
            {
                return _synchronizedProperties ??
                       (_synchronizedProperties = Members.Where(e => e is PropertyRepresentation && e.IsFullySynchronized())
                           .Select(e => e as PropertyRepresentation).ToList());
            }
        }

        private ICollection<PropertyRepresentation> _unSynchronizedProperties;

        public ICollection<PropertyRepresentation> UnSynchronizedProperties
        {
            get
            {
                return _unSynchronizedProperties ??
                       (_unSynchronizedProperties = Members.Where(e => e is PropertyRepresentation && !e.IsFullySynchronized())
                           .Select(e => e as PropertyRepresentation).ToList());
            }
        }


        public ICollection<MethodRepresentation> Methods => Members.OfType<MethodRepresentation>().ToList();
        public ICollection<PropertyRepresentation> Properties => Members.OfType<PropertyRepresentation>().ToList();


        public ClassRepresentation(ClassDeclarationSyntax classDeclarationSyntax)
        {
            Name = classDeclarationSyntax.Identifier;
            Members = new List<IMember>();
            ClassDeclarationSyntax = classDeclarationSyntax;
            FullyQualifiedDomainName = classDeclarationSyntax.Identifier.ToString();
        }

        public IEnumerable<IdentifierNameSyntax> GetIdentifiersInLocks()
        {
            IEnumerable<IdentifierNameSyntax> identifiers = new List<IdentifierNameSyntax>();

            foreach (var memberWithBody in Members)
            {
                identifiers = identifiers.Concat(SyntaxNodeFilter.GetIdentifiersInLocks(memberWithBody.Blocks));
            }
            return identifiers;
        }

        public ExpressionSyntax GetDefaultLockObject()
        {
            var lockExpressions = SyntaxNodeFilter.GetLockStatements(ClassDeclarationSyntax).Select(e => e.Expression).ToList();
            if (lockExpressions == null || !lockExpressions.Any())
            {
                return LockBuilder.DefaultLockObject();
            }
            return lockExpressions.GroupBy(i => i).OrderByDescending(group => group.Count()).Select(group => group.Key).First();
        }

        public bool ClassHasSynchronizedMember()
        {
            return Members.Any(e => e.IsSynchronized());
        }

        public IMember GetMemberByName(string memberName)
        {
            return Members.FirstOrDefault(e => e.Name.ToString() == memberName);
        }

        public  List<IMember> GetMembersWithMultipleLocks()
        {
            var members = new List<IMember>();
            var counter = 0;
            foreach (var memberWithBody in Members)
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

        private static void GetNextDepthLock(IBody block, ICollection<IMember> members, int counter, IMember member)
        {
            if (counter == 2 && !members.Contains(member))
            {
                members.Add(member);
            }

            foreach (var subBlock in block.Blocks)
            {
                if (subBlock is LockBlock)
                {
                    counter++;
                }
                GetNextDepthLock(subBlock, members, counter, member);
            }
        }
    }
}
