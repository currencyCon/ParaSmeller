
using System.Collections.Generic;
using ConcurrencyAnalyzer.SemanticAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConcurrencyAnalyzer.Representation
{
    public class InvocationExpressionRepresentation
    {
        public readonly string CalledClass;
        public readonly SimpleNameSyntax InvocationTargetName;
        public readonly bool Synchronized;
        public readonly InvocationExpressionSyntax Implementation;
        public readonly IBody ContainingBody;
        public IMember InvokedImplementation { get; set; }
        public readonly SymbolKind Type;
        public readonly List<IdentifierNameSyntax> Arguments;
        public readonly string OriginalDefinition;

        public InvocationExpressionRepresentation(bool synchronized, SymbolInformation symbolInfo, InvocationExpressionSyntax implementation, IBody containingBody, SimpleNameSyntax invocationTarget)
        {
            Synchronized = synchronized;
            Arguments = new List<IdentifierNameSyntax>();
            CalledClass = symbolInfo.ClassName;
            OriginalDefinition = symbolInfo.OriginalDefinition;
            Type = symbolInfo.Type;
            Implementation = implementation;
            ContainingBody = containingBody;
            InvocationTargetName = invocationTarget;
        }
    }
}
