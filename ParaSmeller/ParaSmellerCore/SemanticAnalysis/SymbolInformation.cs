
using Microsoft.CodeAnalysis;

namespace ParaSmellerCore.SemanticAnalysis
{
    public class SymbolInformation
    {
        public SymbolKind Type { get; set; }
        public string ClassName { get; set; }
        public string MethodDefinitionWithoutParameters { get; set; }
        public string OriginalDefinition { get; set; }
    }
}
