using System;
using Microsoft.CodeAnalysis;

namespace ParaSmellerCore.SemanticAnalysis
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

        public static TSymbolKind GetDeclaredSymbol<TSymbolKind>(SyntaxNode syntaxNode, SemanticModel semanticModel) where TSymbolKind : ISymbol
        {
            try
            {
                var symbol = semanticModel.GetDeclaredSymbol(syntaxNode);
                return (TSymbolKind)symbol;
            }
            catch (Exception)
            {
                return default(TSymbolKind);
            }
        }
    }
}
