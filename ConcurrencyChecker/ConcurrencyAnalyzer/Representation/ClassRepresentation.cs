using System.Collections.Generic;
using System.Linq;
using ConcurrencyAnalyzer.SyntaxFilters;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConcurrencyAnalyzer.Representation
{
    public class ClassRepresentation
    {
        public ClassDeclarationSyntax ClassDeclarationSyntax;
        public string FullyQualifiedDomainName;
        public SyntaxToken Name { get; set; }
        public ICollection<IMethodRepresentation> Methods { get; set; }
        public ICollection<IPropertyRepresentation> Properties { get; set; }
        public ICollection<IMemberWithBody> Members { get; set; }
        public ClassRepresentation(ClassDeclarationSyntax classDeclarationSyntax)
        {
            Name = classDeclarationSyntax.Identifier;
            Methods = new List<IMethodRepresentation>();
            Properties = new List<IPropertyRepresentation>();
            Members = new List<IMemberWithBody>();
            ClassDeclarationSyntax = classDeclarationSyntax;
            FullyQualifiedDomainName = classDeclarationSyntax.Identifier.ToFullString();
        }

        public IEnumerable<InvocationExpressionRepresentation> GetInvocationsInLocks()
        {
            IEnumerable<InvocationExpressionRepresentation> invocations = new List<InvocationExpressionRepresentation>();

            foreach (var memberWithBody in Members)
            {
                invocations = invocations.Concat(SyntaxNodeFilter.GetInvocationsInLocks(memberWithBody.Blocks));
            }
            return invocations;
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
    }
}
