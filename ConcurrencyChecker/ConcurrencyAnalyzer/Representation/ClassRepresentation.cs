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
        public readonly ICollection<Member> Members;
		public readonly DestructorDeclarationSyntax Destructor;
        public readonly SemanticModel SemanticModel;
        
        public ICollection<MethodRepresentation> SynchronizedMethods { get; set; }
        public ICollection<MethodRepresentation> UnSynchronizedMethods { get; set; }
        public ICollection<PropertyRepresentation> SynchronizedProperties { get; set; }
        public ICollection<PropertyRepresentation> UnSynchronizedProperties { get; set; }
        public ICollection<MethodRepresentation> Methods => Members.OfType<MethodRepresentation>().ToList();
        public ICollection<PropertyRepresentation> Properties => Members.OfType<PropertyRepresentation>().ToList();
        public readonly ICollection<FieldDeclarationSyntax> Fields;
        public INamedTypeSymbol NamedTypeSymbol { get; set; }

        public ClassRepresentation(ClassDeclarationSyntax classDeclarationSyntax, SemanticModel semanticModel)
        {
            Name = classDeclarationSyntax.Identifier;
            SemanticModel = semanticModel;
            Members = new List<Member>();
            Implementation = classDeclarationSyntax;
            Destructor = Implementation.GetFirstChild<DestructorDeclarationSyntax>();
            Fields = Implementation.GetChildren<FieldDeclarationSyntax>().ToList();
            NamedTypeSymbol = semanticModel.GetDeclaredSymbol(Implementation) ;
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
        
        public  List<Member> GetMembersWithMultipleLocks()
        {
            var members = new List<Member>();
            foreach (var memberWithBody in Members)
            {
                foreach (var block in memberWithBody.Blocks)
                {
                    GetNextDeeperLock(block, members, memberWithBody);
                }
            }

            return members;
        }

        private static void GetNextDeeperLock(Body block, ICollection<Member> members, Member member)
        {
            if (!members.Contains(member))
            {
                members.Add(member);
            }

            foreach (var subBlock in block.Blocks)
            {
                GetNextDeeperLock(subBlock, members, member);
            }
        }

        public bool IsStaticDefinedLockObject(LockStatementSyntax lockStatement)
        {
            return Fields.Any(field => field.DeclaresVariable(lockStatement.Expression.ToString(), new[] {SyntaxFactory.Token(SyntaxKind.StaticKeyword).ToString()}));
        }
    }
}
