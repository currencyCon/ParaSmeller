

using System;
using Microsoft.CodeAnalysis;

namespace ConcurrencyAnalyzer.SemanticAnalysis
{
    public static class SymbolInspector
    {
        public static TSymbolKind GetSpecializedSymbol<TSymbolKind>(SyntaxNode syntaxNode, SemanticModel semanticModel) where TSymbolKind : ISymbol
        {
            try
            {
                var symbol = semanticModel.GetSymbolInfo(syntaxNode);
                return (TSymbolKind)symbol.Symbol;
            }
            catch (Exception)
            {
                return default(TSymbolKind);
            }
        }
    }
}
