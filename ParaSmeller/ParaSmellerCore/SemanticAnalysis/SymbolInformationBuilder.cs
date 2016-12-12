using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ParaSmellerCore.SemanticAnalysis
{
    public static class SymbolInformationBuilder
    {
        private const string NameSpaceSepatator = ".";
        public static SymbolInformation Create(SimpleNameSyntax simpleNameSyntax, SemanticModel semanticModel)
        {
            var symbolInfo = semanticModel.GetSymbolInfo(simpleNameSyntax);
            var symbol = symbolInfo.Symbol;
            try
            {
                return new SymbolInformation
                {
                    ClassName = symbol == null ? DefaultSymbolValues.ClassName : GetClassName(symbol),
                    MethodDefinitionWithoutParameters = symbol == null ? DefaultSymbolValues.MethodDefinitionWithoutParameters : GetMethodDefinitionWithoutParameters(symbol),
                    OriginalDefinition = symbol == null ? DefaultSymbolValues.OriginalDefinition : symbol.OriginalDefinition.ToString(),
                    Type = symbol == null ? DefaultSymbolValues.Type : GetType(symbol)
                };
            }
            catch (Exception)
            {
                return new SymbolInformation
                {
                    ClassName = DefaultSymbolValues.ClassName,
                    MethodDefinitionWithoutParameters = DefaultSymbolValues.MethodDefinitionWithoutParameters,
                    OriginalDefinition = DefaultSymbolValues.OriginalDefinition,
                    Type = DefaultSymbolValues.Type
                };
            }
        }
        
        private static SymbolKind GetType(ISymbol symbol)
        {
            return symbol.Kind;
        }

        private static string GetClassName(ISymbol symbol)
        {
            return symbol.ContainingType.Name;
        }

        private static string GetMethodDefinitionWithoutParameters(ISymbol symbol)
        {
            return symbol.ContainingType.OriginalDefinition + NameSpaceSepatator + symbol.Name;
        }
    }
}
