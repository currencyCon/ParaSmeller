using System.Collections.Generic;
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
    }
}
