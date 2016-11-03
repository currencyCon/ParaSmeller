using System.Collections.Generic;
using System.Linq;
using ConcurrencyAnalyzer.Builders;
using ConcurrencyAnalyzer.SyntaxNodeUtils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConcurrencyAnalyzer.Representation
{
    public class ClassRepresentation
    {
        public readonly ClassDeclarationSyntax Implementation;
        public readonly SyntaxToken Name;
        public readonly ICollection<IMember> Members;
		public readonly DestructorDeclarationSyntax Destructor;
        private const int ThresholdMaxDepthAsync = 3;

        public ICollection<MethodRepresentation> SynchronizedMethods { get; set; }
        public ICollection<MethodRepresentation> UnSynchronizedMethods { get; set; }
        public ICollection<PropertyRepresentation> SynchronizedProperties { get; set; }
        public ICollection<PropertyRepresentation> UnSynchronizedProperties { get; set; }
        public ICollection<MethodRepresentation> Methods => Members.OfType<MethodRepresentation>().ToList();
        public ICollection<PropertyRepresentation> Properties => Members.OfType<PropertyRepresentation>().ToList();
        public readonly ICollection<FieldDeclarationSyntax> Fields;

        public ClassRepresentation(ClassDeclarationSyntax classDeclarationSyntax)
        {
            Name = classDeclarationSyntax.Identifier;
            Members = new List<IMember>();
            Implementation = classDeclarationSyntax;
            Destructor = Implementation.GetFirstChild<DestructorDeclarationSyntax>();
            Fields = Implementation.GetChildren<FieldDeclarationSyntax>().ToList();
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
            var lockExpressions = SyntaxNodeFilter.GetLockStatements(Implementation).Select(e => e.Expression).ToList();
            if (lockExpressions == null || !lockExpressions.Any())
            {
                return LockBuilder.DefaultLockObject();
            }
            return lockExpressions.GroupBy(i => i).OrderByDescending(group => group.Count()).Select(group => group.Key).First();
        }

        public bool HasSynchronizedMember()
        {
            return Members.Any(e => e.IsFullySynchronized());
        }

        public IMember GetMemberByName(string memberName)
        {
            return Members.FirstOrDefault(e => e.Name.ToString() == memberName);
        }

        public  List<IMember> GetMembersWithMultipleLocks()
        {
            var members = new List<IMember>();
            var counter = 1;
            foreach (var memberWithBody in Members)
            {
                foreach (var block in memberWithBody.Blocks)
                {
                    if (block is LockBlock)
                    {
                        counter++;
                    }
                    GetNextDeeperLock(block, members, counter, memberWithBody);
                }
            }

            return members;
        }

        private static void GetNextDeeperLock(IBody block, ICollection<IMember> members, int counter, IMember member)
        {
            if (counter == ThresholdMaxDepthAsync && !members.Contains(member))
            {
                members.Add(member);
            }

            foreach (var subBlock in block.Blocks)
            {
                if (subBlock is LockBlock)
                {
                    counter++;
                }
                GetNextDeeperLock(subBlock, members, counter, member);
            }
        }

        public bool IsStaticDefinedLockObject(LockStatementSyntax lockStatement)
        {
            return Fields.Any(field => field.DeclaresVariable(lockStatement.Expression.ToString(), new[] {SyntaxFactory.Token(SyntaxKind.StaticKeyword).ToString()}));
        }
    }
}
