using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ExplicitThreadsChecker
{
    internal static class SyntaxNodeExtensions
    {
        public static IEnumerable<VariableDeclaratorSyntax> GetLocalDeclaredVariables(this SyntaxNode root)
        {
            return root.DescendantNodes()
                .OfType<LocalDeclarationStatementSyntax>()
                .SelectMany(b => b.DescendantNodes().OfType<VariableDeclaratorSyntax>());
        }

        public static VariableDeclaratorSyntax SingleVariable(this IEnumerable<VariableDeclaratorSyntax> variables,
            string variableName)
        {
            return variables.First(c => c.Identifier.ToString() == variableName);
        } 

    }
}
