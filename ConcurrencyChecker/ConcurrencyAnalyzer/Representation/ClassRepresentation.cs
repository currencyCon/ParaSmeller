using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConcurrencyAnalyzer.Representation
{
    public class ClassRepresentation
    {
        public ClassDeclarationSyntax ClassDeclarationSyntax;
        public string FullyQualifiedDomainName;
        public ICollection<IMethodRepresentation> Methods { get; set; }
        public ClassRepresentation(ClassDeclarationSyntax classDeclarationSyntax)
        {
            Methods = new List<IMethodRepresentation>();
            ClassDeclarationSyntax = classDeclarationSyntax;
            FullyQualifiedDomainName = classDeclarationSyntax.Identifier.ToFullString();
        }
    }
}
