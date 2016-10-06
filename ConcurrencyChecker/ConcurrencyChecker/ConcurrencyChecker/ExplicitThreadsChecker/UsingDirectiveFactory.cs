using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ExplicitThreadsChecker
{
    internal class UsingDirectiveFactory
    {
        internal static UsingDirectiveSyntax Create(string usingName)
        {
            NameSyntax qualifiedName = null;

            foreach (var identifier in usingName.Split('.'))
            {
                var name = SyntaxFactory.IdentifierName(identifier);

                if (qualifiedName != null)
                {
                    qualifiedName = SyntaxFactory.QualifiedName(qualifiedName, name);
                }
                else
                {
                    qualifiedName = name;
                }
            }
            return SyntaxFactory.UsingDirective(qualifiedName);
        }
    }
}