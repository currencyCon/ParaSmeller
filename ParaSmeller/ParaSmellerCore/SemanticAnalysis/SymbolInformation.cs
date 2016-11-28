
using Microsoft.CodeAnalysis;

namespace ParaSmellerCore.SemanticAnalysis
{
    public class SymbolInformation
    {
        public SymbolKind Type { get; set; }
        public string ClassName { get; set; }
        public string OriginalDefinition { get; set; }
        public string Definition { get; set; }
    }
}
