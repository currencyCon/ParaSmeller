using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConcurrencyAnalyzer.Representation
{
    public class MethodRepresentation : Member
    {
        public readonly MethodDeclarationSyntax Implementation;
        public readonly ICollection<ParameterSyntax> Parameters;

        private MethodRepresentation(MethodDeclarationSyntax methodDeclarationSyntax, string originalDefinition, SemanticModel semanticModel): base(originalDefinition, methodDeclarationSyntax.Identifier, semanticModel)
        {
            Implementation = methodDeclarationSyntax;
            Parameters = Implementation.ParameterList.Parameters.ToList();
        }
        public MethodRepresentation(MethodDeclarationSyntax methodDeclarationSyntax, ClassRepresentation classRepresentation, string originalDefintion, SemanticModel semanticModel): this(methodDeclarationSyntax, originalDefintion, semanticModel)
        {
            ContainingClass = classRepresentation;
        }

        public MethodRepresentation(MethodDeclarationSyntax methodDeclarationSyntax, InterfaceRepresentation interfaceRepresentation, string originalDefintion, SemanticModel semanticModel): this(methodDeclarationSyntax, originalDefintion, semanticModel)
        {
            ContainingInterface = interfaceRepresentation;
        }

        public override bool IsFullySynchronized()
        {
            if (HasStandardMethodBody())
            {
                var methodBody = Blocks.First();
                return methodBody.Blocks.Any() && methodBody.Blocks.All(e => e.IsSynchronized);
            }
            return false;
        }

        private bool HasStandardMethodBody()
        {
            return Blocks.Count == 1 && !Blocks.First().IsSynchronized;
        }

        public bool MethodHasHalfSynchronizedProperties()
        {
            var methodsWithHalfSynchronizedProperties = ContainingClass.GetMethodsWithHalfSynchronizedProperties();
            return methodsWithHalfSynchronizedProperties.Select(e => e.Name.Text).Contains(Name.Text);
        }
    }
}
