using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConcurrencyAnalyzer.Representation
{
    public interface IPropertyRepresentation : IMemberWithBody
    {
        PropertyDeclarationSyntax PropertyImplementation { get; set; }
        AccessorDeclarationSyntax Getter { set; get; }
        AccessorDeclarationSyntax Setter { get; set; }
    }
}
