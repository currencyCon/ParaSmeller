using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConcurrencyAnalyzer.Representation
{
    public class InterfaceRepresentation
    {
        public readonly InterfaceDeclarationSyntax Implementation;
        public readonly SyntaxToken Name;
        public readonly ICollection<Member> Members = new List<Member>();
		public readonly SemanticModel SemanticModel;
        public readonly INamedTypeSymbol NamedTypeSymbol;
        public readonly ICollection<ClassRepresentation> ImplementingClasses = new List<ClassRepresentation>();

        public ICollection<MethodRepresentation> Methods => Members.OfType<MethodRepresentation>().ToList();
        public ICollection<PropertyRepresentation> Properties => Members.OfType<PropertyRepresentation>().ToList();

        public InterfaceRepresentation(InterfaceDeclarationSyntax interfaceDeclarationSyntax, SemanticModel semanticModel)
        {
            Name = interfaceDeclarationSyntax.Identifier;
            SemanticModel = semanticModel;
            Implementation = interfaceDeclarationSyntax;
            NamedTypeSymbol = semanticModel.GetDeclaredSymbol(Implementation);
        }
        
    }
}
