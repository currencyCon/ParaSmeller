using System.Collections.Generic;

namespace ParaSmellerCore.RepresentationFactories.AnalysisConfiguration
{
    public static class AnalysisConfiguration
    {
        public static readonly List<string> NamesSpacesToExclude = new List<string> { "System", "object", "string", "decimal", "int", "double", "float", "Antlr", "long", "char", "bool", "byte", "short", "uint", "ulong", "ushort", "sbyte" };
    }
}
