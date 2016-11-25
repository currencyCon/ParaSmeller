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
        public readonly ICollection<FieldDeclarationSyntax> Fields;
        public readonly INamedTypeSymbol NamedTypeSymbol;

        public ICollection<MethodRepresentation> SynchronizedMethods { get; set; }
        public ICollection<MethodRepresentation> UnSynchronizedMethods { get; set; }
        public ICollection<PropertyRepresentation> SynchronizedProperties { get; set; }
        public ICollection<PropertyRepresentation> UnSynchronizedProperties { get; set; }
        public ICollection<MethodRepresentation> Methods => Members.OfType<MethodRepresentation>().ToList();
        public ICollection<PropertyRepresentation> Properties => Members.OfType<PropertyRepresentation>().ToList();
        public Dictionary<string, List<ClassRepresentation>> ClassMap { get; set; }
        public Dictionary<string, InterfaceRepresentation> InterfaceMap { get; set; }

        public ClassRepresentation(ClassDeclarationSyntax classDeclarationSyntax, SemanticModel semanticModel)
        {
            Name = classDeclarationSyntax.Identifier;
            SemanticModel = semanticModel;
            Members = new List<Member>();
            Implementation = classDeclarationSyntax;
            Destructor = Implementation.GetFirstChild<DestructorDeclarationSyntax>();
            Fields = Implementation.GetChildren<FieldDeclarationSyntax>().ToList();
            NamedTypeSymbol = semanticModel.GetDeclaredSymbol(Implementation) ;
            ClassMap = new Dictionary<string, List<ClassRepresentation>>();
            InterfaceMap = new Dictionary<string, InterfaceRepresentation>();
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

        public IEnumerable<MethodRepresentation> GetMethodsWithHalfSynchronizedProperties()
        {
            var methodsWithHalfSynchronizedProperties = new List<MethodRepresentation>();
            var identifiersInSyncedMethods = SynchronizedMethods.SelectMany(e => SyntaxNodeFilter.GetIdentifiersInLocks(e.Blocks)).Select(e => e.Identifier.Text);
            var unsProp = UnSynchronizedProperties.Where(e => identifiersInSyncedMethods.Contains(e.Name.Text)).ToList();
            foreach (var unsyncedMethod in UnSynchronizedMethods)
            {
                var identifiersInMethods = unsyncedMethod.GetChildren<IdentifierNameSyntax>().Select(e => e.Identifier.Text);
                if (unsProp.Select(e => e.Name.Text).Any(e => identifiersInMethods.Contains(e)))
                {
                    methodsWithHalfSynchronizedProperties.Add(unsyncedMethod);
                }
            }
            return methodsWithHalfSynchronizedProperties;
        }

        public bool ImplementsInvocationDefinedInBaseClasses(InvocationExpressionRepresentation invocationExpressionRepresentation)
        {
            foreach(var classes in ClassMap.Values)
            {
                foreach (var baseClass in classes)
                {
                    if (baseClass == null) continue;
                    foreach (var member in baseClass.Members)
                    {
                        if (member.OriginalDefinition == invocationExpressionRepresentation.Defintion)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public bool ImplementsInvocationDefinedInInterfaces(InvocationExpressionRepresentation invocationExpressionRepresentation)
        {
            foreach (var interfacee in InterfaceMap.Values)
            {
                if (interfacee == null) continue;
                foreach (var member in interfacee.Members)
                {
                    if (member.OriginalDefinition == invocationExpressionRepresentation.Defintion)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
