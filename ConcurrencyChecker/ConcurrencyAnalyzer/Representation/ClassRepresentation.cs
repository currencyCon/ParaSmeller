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
        public ClassDeclarationSyntax ClassDeclarationSyntax { get; set; }
        public string FullyQualifiedDomainName { get; set; }
        public SyntaxToken Name { get; set; }
        private ICollection<IMethodRepresentation> _synchronizedMethods;

        public ICollection<IMethodRepresentation> SynchronizedMethods
        {
            get
            {
                return _synchronizedMethods ??
                       (_synchronizedMethods = Members.Where(e => e is IMethodRepresentation && e.IsFullySynchronized())
                           .Select(e => e as IMethodRepresentation).ToList());
            }
        }

        private ICollection<IMethodRepresentation> _unSynchronizedMethods;

        public ICollection<IMethodRepresentation> UnSynchronizedMethods
        {
            get
            {
                return _unSynchronizedMethods ??
                       (_unSynchronizedMethods = Members.Where(e => e is IMethodRepresentation && !e.IsFullySynchronized())
                           .Select(e => e as IMethodRepresentation).ToList());
            }
        }




        private ICollection<IPropertyRepresentation> _synchronizedProperties;

        public ICollection<IPropertyRepresentation> SynchronizedProperties
        {
            get
            {
                return _synchronizedProperties ??
                       (_synchronizedProperties = Members.Where(e => e is IPropertyRepresentation && e.IsFullySynchronized())
                           .Select(e => e as IPropertyRepresentation).ToList());
            }
        }

        private ICollection<IPropertyRepresentation> _unSynchronizedProperties;

        public ICollection<IPropertyRepresentation> UnSynchronizedProperties
        {
            get
            {
                return _unSynchronizedProperties ??
                       (_unSynchronizedProperties = Members.Where(e => e is IPropertyRepresentation && !e.IsFullySynchronized())
                           .Select(e => e as IPropertyRepresentation).ToList());
            }
        }


        public ICollection<IMethodRepresentation> Methods => Members.OfType<IMethodRepresentation>().ToList();
        public ICollection<IPropertyRepresentation> Properties => Members.OfType<IPropertyRepresentation>().ToList();
        public ICollection<IMember> Members { get; set; }


        public ClassRepresentation(ClassDeclarationSyntax classDeclarationSyntax)
        {
            Name = classDeclarationSyntax.Identifier;
            Members = new List<IMember>();
            ClassDeclarationSyntax = classDeclarationSyntax;
            FullyQualifiedDomainName = classDeclarationSyntax.Identifier.ToFullString();
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
