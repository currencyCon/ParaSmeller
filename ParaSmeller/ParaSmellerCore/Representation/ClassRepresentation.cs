using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ParaSmellerCore.Builders;
using ParaSmellerCore.SyntaxNodeUtils;

namespace ParaSmellerCore.Representation
{
    public class ClassRepresentation
    {
        public readonly ClassDeclarationSyntax Implementation;
        public readonly SyntaxToken Name;
        public readonly ConcurrentBag<Member> Members = new ConcurrentBag<Member>();
		public readonly DestructorDeclarationSyntax Destructor;
        public readonly SemanticModel SemanticModel;
        public readonly ICollection<FieldDeclarationSyntax> Fields;
        public readonly INamedTypeSymbol NamedTypeSymbol;
        public readonly ConcurrentDictionary<string, ConcurrentBag<ClassRepresentation>> ClassMap = new ConcurrentDictionary<string, ConcurrentBag<ClassRepresentation>>();
        public readonly ConcurrentDictionary<string, InterfaceRepresentation> InterfaceMap = new ConcurrentDictionary<string, InterfaceRepresentation>();

        public ICollection<MethodRepresentation> SynchronizedMethods { get; set; }
        public ICollection<MethodRepresentation> UnSynchronizedMethods { get; set; }
        public ICollection<PropertyRepresentation> SynchronizedProperties { get; set; }
        public ICollection<PropertyRepresentation> UnSynchronizedProperties { get; set; }
        public ICollection<MethodRepresentation> Methods => Members.OfType<MethodRepresentation>().ToList();
        public ICollection<PropertyRepresentation> Properties => Members.OfType<PropertyRepresentation>().ToList();


        public ClassRepresentation(ClassDeclarationSyntax classDeclarationSyntax, SemanticModel semanticModel)
        {
            Name = classDeclarationSyntax.Identifier;
            SemanticModel = semanticModel;
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
                    block.AddInvokedMembersWithLock(members, memberWithBody);
                }
            }

            return members;
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

        public void AddClassInHierarchy(string baseClassOriginalDefinition, ClassRepresentation baseClassRepresentation)
        {
            var classList = ClassMap.GetOrAdd(baseClassOriginalDefinition, new ConcurrentBag<ClassRepresentation>());
            classList.Add(baseClassRepresentation);
        }

        public  void AddInterface(string interfaceOriginalDefinition, InterfaceRepresentation interfaceRepresentation)
        {
            if (!InterfaceMap.TryAdd(interfaceOriginalDefinition, interfaceRepresentation))
            {
                Logger.Debug($"Tried to add Interface twice: {interfaceOriginalDefinition}");
            }
        }
    }
}
