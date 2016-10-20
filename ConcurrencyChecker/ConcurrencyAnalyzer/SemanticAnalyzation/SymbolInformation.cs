
using Microsoft.CodeAnalysis;

namespace ConcurrencyAnalyzer.SemanticAnalyzation
{
    public class SymbolInformation
    {
        public SymbolKind Type { get; set; }
        public string ClassName { get; set; }
        public string OriginalDefinition { get; set; }
    }
}
