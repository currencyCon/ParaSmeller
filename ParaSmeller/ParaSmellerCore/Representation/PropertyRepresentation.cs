using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ParaSmellerCore.SyntaxNodeUtils;

namespace ParaSmellerCore.Representation
{
    public class PropertyRepresentation : Member
    {
        private const string GetKeyWord = "get";
        private const string SetKeyWord = "set";
        public readonly BlockSyntax Getter;
        public readonly BlockSyntax Setter;
        public readonly PropertyDeclarationSyntax Implementation;

        protected PropertyRepresentation(PropertyDeclarationSyntax propertyDeclarationSyntax, string originalDefinition, SemanticModel semanticModel): base(originalDefinition, propertyDeclarationSyntax.Identifier, semanticModel)
        {
            Implementation = propertyDeclarationSyntax;
            Getter = propertyDeclarationSyntax.AccessorList?.Accessors.FirstOrDefault(e => e.Keyword.ToString() == GetKeyWord)?.Body;
            Setter = propertyDeclarationSyntax.AccessorList?.Accessors.FirstOrDefault(e => e.Keyword.ToString() == SetKeyWord)?.Body;
        }
        public PropertyRepresentation(PropertyDeclarationSyntax propertyDeclarationSyntax, ClassRepresentation classRepresentation, string originalDefintion, SemanticModel semanticModel): this(propertyDeclarationSyntax, originalDefintion, semanticModel)
        {
            ContainingClass = classRepresentation;
        }

        public PropertyRepresentation(PropertyDeclarationSyntax propertyDeclarationSyntax, InterfaceRepresentation interfaceRepresentation, string originalDefintion, SemanticModel semanticModel): this(propertyDeclarationSyntax, originalDefintion, semanticModel)
        {
            ContainingInterface = interfaceRepresentation;
        }

        public override bool IsFullySynchronized()
        {
            return AllAccessorsAreSynchronized();
        }

        private bool AllAccessorsAreSynchronized()
        {
            if (!Blocks.Any())
            {
                return false;
            }
            var isFullySynchronized = true;
            foreach (var accessor in Blocks)
            {
                if (!AccessorIsFullySynchronized(accessor))
                {
                    isFullySynchronized = false;
                }
            }
            return isFullySynchronized;
        }

        private static bool AccessorIsFullySynchronized(Body block)
        {
            return block.Blocks.Count == 1 && block.Blocks.First().IsSynchronized;
        }

        public bool NeedsSynchronization()
        {
            if (IsFullySynchronized())
            {
                return false;
            }
            if (SyntaxNodeFilter.ReturnsConstantValue(Getter))
            {
                return false;
            }
            var identifiersInLockStatements = ContainingClass.GetIdentifiersInLocks().Select(e => e.Identifier.ToString());
            return identifiersInLockStatements.Contains(Implementation.Identifier.Text);
        }
    }
}
