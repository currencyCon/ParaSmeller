using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ConcurrencyAnalyzer.Builders;

namespace ConcurrencyChecker.ExplicitThreadsChecker
{
    public static class UsingHandler
    {
        internal static SyntaxNode AddUsingIfNotExists(SyntaxNode root, string usingName)
        {
            var compilationUnit = (CompilationUnitSyntax) root;

            var exists = compilationUnit.Usings.Any(u => u.Name.ToString() == usingName);
            if (!exists)
            {
                var usingSystemThreadingTask = UsingDirectiveBuilder.Create(usingName);
                compilationUnit = compilationUnit.AddUsings(usingSystemThreadingTask);
            }

            return compilationUnit;
        }
    }
}