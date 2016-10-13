using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConcurrencyAnalyzer.Representation
{
    public interface IPropertyRepresentation : IMemberWithBody
    {
        PropertyDeclarationSyntax PropertyImplementation { get; set; }
        BlockSyntax Getter { set; get; }
        BlockSyntax Setter { get; set; }
    }
}
