using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ParaSmellerCore.SyntaxNodeUtils
{
    public static class SyntaxNodeExtensions
    {
        public static TChild GetFirstChild<TChild>(this SyntaxNode node)
        {
            return GetChildren<TChild>(node).FirstOrDefault();
        }
        public static IEnumerable<TChildren> GetChildren<TChildren>(this SyntaxNode node)
        {
            if (node == null)
            {
                return new List<TChildren>();
            }
            return node.DescendantNodesAndSelf().OfType<TChildren>();
        }

        public static TParent GetFirstParent<TParent>(this SyntaxNode node)
        {
            return GetParents<TParent>(node).FirstOrDefault();
        }

        public static bool IsInTopLevelBlock(this SyntaxNode node)
        {
            return node.GetFirstParent<BlockSyntax>().Parent is MethodDeclarationSyntax;
        }

        public static IEnumerable<TParents> GetParents<TParents>(this SyntaxNode node)
        {
            return node.AncestorsAndSelf().OfType<TParents>();
        }

        public static IEnumerable<TChildren> GetDirectChildren<TChildren>(this SyntaxNode node)
        {
            return node.ChildNodes().OfType<TChildren>();
        }

        public static bool IsSynchronized(this SyntaxNode node)
        {
            return node.GetChildren<LockStatementSyntax>().Any();
        }

        public static IEnumerable<MemberAccessExpressionSyntax> GetInvocationExpression(this SyntaxNode node, string className, string methodName)
        {
            return node.GetChildren<MemberAccessExpressionSyntax>().Where(e => e.Expression.ToString() == className && e.Name.ToString() == methodName);
        }

        public static bool DeclaresVariable(this FieldDeclarationSyntax field, string variableName)
        {
            return field.Declaration.Variables.Any(e => e.Identifier.Text == variableName);        
        }

        public static bool DeclaresVariable(this FieldDeclarationSyntax field, string variableName, string[] modifiers)
        {
            return field.DeclaresVariable(variableName) && !modifiers.Except(field.Modifiers.Select(e => e.Text)).Any();
        }
    }
}
