using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConcurrencyAnalyzer.Builders
{
    public static class SyntaxFormatter
    {
        private static IEnumerable<SyntaxTrivia> AddIndention(int level, IEnumerable<SyntaxTrivia> indention)
        {
            var oneTabIndention = new List<SyntaxTrivia>
            {
                SyntaxFactory.Space,
                SyntaxFactory.Space,
                SyntaxFactory.Space,
                SyntaxFactory.Space
            };
            for (var i = 0; i < level; i++)
            {
                indention = indention.Concat(oneTabIndention);
            }
            return indention;
        }

        public static SyntaxToken AddIndention(SyntaxToken closeBraceToken, int level)
        {
            IEnumerable<SyntaxTrivia> indention = closeBraceToken.LeadingTrivia.ToList();
            indention = AddIndention(level, indention);
            return closeBraceToken.WithLeadingTrivia(indention);
        }

        public static StatementSyntax AddIndention(StatementSyntax statementSyntax, int level)
        {
            IEnumerable<SyntaxTrivia> indention = statementSyntax.GetLeadingTrivia().ToList();
            indention = AddIndention(level, indention);
            return statementSyntax.WithLeadingTrivia(indention);
        }
    }
}
