using System.Collections.Generic;
using System.Linq;
using ConcurrencyAnalyzer.SemanticAnalysis;
using ConcurrencyAnalyzer.SyntaxNodeUtils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConcurrencyAnalyzer.Representation
{
    public class InvocationExpressionRepresentation
    {
        public readonly string CalledClass;
        public readonly string CalledClassOriginal;
        public readonly SimpleNameSyntax InvocationTargetName;
        public readonly bool IsSynchronized;
        public readonly InvocationExpressionSyntax Implementation;
        public readonly Body ContainingBody;
        public readonly SymbolKind Type;
        public readonly List<IdentifierNameSyntax> Arguments;
        public readonly string OriginalDefinition;
        public readonly string Defintion;
        public readonly bool IsInvokedInTask;
        public readonly List<Member> InvokedImplementations;
        
        public InvocationExpressionRepresentation(bool isSynchronized, SymbolInformation symbolInfo, InvocationExpressionSyntax implementation, Body containingBody, SimpleNameSyntax invocationTarget, bool isInvokedInTask)
        {
            IsSynchronized = isSynchronized;
            Arguments = new List<IdentifierNameSyntax>();
            CalledClass = symbolInfo.ClassName;
            OriginalDefinition = symbolInfo.OriginalDefinition;
            Type = symbolInfo.Type;
            Implementation = implementation;
            ContainingBody = containingBody;
            InvocationTargetName = invocationTarget;
            IsInvokedInTask = isInvokedInTask;
            InvokedImplementations = new List<Member>();
            Defintion = symbolInfo.Definition;
            var splittedDefinition = Defintion.Split('.');
            var classParts = splittedDefinition.Take(splittedDefinition.Length - 1);
            var classDefinition = string.Join(".", classParts);
            CalledClassOriginal = classDefinition;
        }

        public TParent GetFirstParent<TParent>()
        {
            return Implementation.GetFirstParent<TParent>();
        }
    }
}
