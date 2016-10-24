using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConcurrencyChecker.ExplicitThreadsChecker
{
    internal static class UsingHandler
    {
        internal static SyntaxNode AddUsingIfNotExists(SyntaxNode root, string usingName)
        {
            var compilationUnit = (CompilationUnitSyntax) root;

            var exists = compilationUnit.Usings.Any(u => u.Name.ToString() == usingName);
            if (!exists)
            {
                var usingSystemThreadingTask = UsingDirectiveFactory.Create(usingName);
                compilationUnit = compilationUnit.AddUsings(usingSystemThreadingTask);
            }

            return compilationUnit;
        }
    }
}