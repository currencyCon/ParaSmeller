using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConcurrencyAnalyzer.Representation
{
    public class MethodRepresentation : Member
    {
        public MethodDeclarationSyntax Implementation { get; set; }
        public ICollection<ParameterSyntax> Parameters { get; set; }
        public MethodRepresentation(MethodDeclarationSyntax methodDeclarationSyntax, ClassRepresentation classRepresentation, string originalDefintion)
        {
            Name = methodDeclarationSyntax.Identifier;
            Parameters = methodDeclarationSyntax.ParameterList.Parameters.ToList();
            InvocationExpressions = new List<InvocationExpressionRepresentation>();
            Implementation = methodDeclarationSyntax;
            Blocks = new List<Body>();
            ContainingClass = classRepresentation;
            Callers = new List<InvocationExpressionRepresentation>();
            OriginalDefinition = originalDefintion;
        }

        public MethodRepresentation(MethodDeclarationSyntax methodDeclarationSyntax, InterfaceRepresentation interfaceRepresentation, string originalDefintion)
        {
            Name = methodDeclarationSyntax.Identifier;
            Parameters = methodDeclarationSyntax.ParameterList.Parameters.ToList();
            InvocationExpressions = new List<InvocationExpressionRepresentation>();
            Implementation = methodDeclarationSyntax;
            Blocks = new List<Body>();
            ContainingInterface = interfaceRepresentation;
            Callers = new List<InvocationExpressionRepresentation>();
            OriginalDefinition = originalDefintion;
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
    }
}
