using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConcurrencyAnalyzer.Representation
{
    public class MethodRepresentation : IMethodRepresentation
    {
        public ICollection<IInvocationExpressionRepresentation> InvocationExpressions { get; set; }
        public ClassRepresentation ContainingClass { get; set; }
        public ICollection<IBody> Blocks { get; set; }
        public SyntaxToken Name { get; set; }
        public bool IsFullySynchronized()
        {
            if (Blocks.Count == 1 && !Blocks.First().IsSynchronized)
            {
                var methodBody = Blocks.First();
                return methodBody.Blocks.Any() && methodBody.Blocks.All(e => e.IsSynchronized);
            }
            return false;
        }

        public ICollection<IInvocationExpressionRepresentation> Callers { get; set; }

        public MethodDeclarationSyntax MethodImplementation { get; set; }
        public ICollection<ParameterSyntax> Parameters { get; set; }

        public MethodRepresentation(MethodDeclarationSyntax methodDeclarationSyntax, ClassRepresentation classRepresentation)
        {
            Name = methodDeclarationSyntax.Identifier;
            Parameters = methodDeclarationSyntax.ParameterList.Parameters.ToList();
            InvocationExpressions = new List<IInvocationExpressionRepresentation>();
            MethodImplementation = methodDeclarationSyntax;
            Blocks = new List<IBody>();
            ContainingClass = classRepresentation;
            Callers = new List<IInvocationExpressionRepresentation>();
        }
    }
}
