using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConcurrencyAnalyzer.SemanticAnalysis
{
    public static class SymbolInformationBuilder
    {
        private const string NameSpaceSepatator = ".";
        public static SymbolInformation Create(SimpleNameSyntax simpleNameSyntax, SemanticModel semanticModel)
        {
            var symbolInfo = semanticModel.GetSymbolInfo(simpleNameSyntax);
            var symbol = symbolInfo.Symbol;
            return new SymbolInformation
            {
                ClassName = symbol == null? DefaultSymbolValues.ClassName : GetClassName(symbol),
                OriginalDefinition = symbol == null ? DefaultSymbolValues.OriginalDefinition : GetOriginalDefinition(symbol),
                Type = symbol == null ? DefaultSymbolValues.Type : GetType(symbol)
            };
        }
        
        private static SymbolKind GetType(ISymbol symbol)
        {
            return symbol.Kind;
        }

        private static string GetClassName(ISymbol symbol)
        {
            return symbol.ContainingType.Name;
        }
        private static string GetOriginalDefinition(ISymbol symbol)
        {
            return symbol.ContainingType.OriginalDefinition + NameSpaceSepatator + symbol.Name;
        }
    }
}
