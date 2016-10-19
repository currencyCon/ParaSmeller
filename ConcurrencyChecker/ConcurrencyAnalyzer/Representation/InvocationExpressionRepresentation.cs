
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConcurrencyAnalyzer.Representation
{
    public class InvocationExpressionRepresentation: IInvocationExpressionRepresentation
    {
        public string CalledClass { get; set; }
        public SimpleNameSyntax InvocationTargetName { get; set; }
        public bool Synchronized { get; set; }
        public InvocationExpressionSyntax Implementation { get; set; }
        public IBody ContainingBody { get; set; }
        public IMemberWithBody InvocationImplementation { get; set; }
        public SymbolKind Type { get; set; }
        public ICollection<IdentifierNameSyntax> Arguments { get; set; }
        public string OriginalDefinition { get; set; }
    }
}
