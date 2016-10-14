using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConcurrencyAnalyzer.Builders
{
    public class PropertyBuilder
    {
        public static FieldDeclarationSyntax BuildBackingField(PropertyDeclarationSyntax property)
        {
            var fieldName = "_" + property.Identifier.Text;
            var variableDeclaration = SyntaxFactory.VariableDeclaration(property.Type, SyntaxFactory.SeparatedList(new[] { SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(fieldName)) }));
            var backingField = SyntaxFactory.FieldDeclaration(variableDeclaration).AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword).WithTrailingTrivia(SyntaxTriviaList.Create(SyntaxFactory.Space)));

            return backingField.WithLeadingTrivia(property.GetLeadingTrivia());
        }

        private static AccessorDeclarationSyntax GetDefaultSetter(BaseFieldDeclarationSyntax backingField)
        {
            var ident = SyntaxFactory.IdentifierName(backingField.Declaration.Variables.First().Identifier.ToString());
            var assignment = SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, ident,
                SyntaxFactory.IdentifierName("value"));
            var expressionStatement = SyntaxFactory.ExpressionStatement(assignment);
            var block = SyntaxFactory.Block(expressionStatement);
            var setter = SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration, LockBuilder.BuildLockBlock(block));
            return setter;
        }
        
        private static AccessorDeclarationSyntax GetDefaultGetter(BaseFieldDeclarationSyntax backingField)
        {
            var body = SyntaxFactory.Block(
                SyntaxFactory.List(new[]
                {
                    SyntaxFactory.ReturnStatement(
                        SyntaxFactory.IdentifierName(backingField.Declaration.Variables.First().Identifier)
                            .WithLeadingTrivia(SyntaxTriviaList.Create(SyntaxFactory.Space)))
                })
                );
            var getter = SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration, LockBuilder.BuildLockBlock(body));
            return getter;
        }

        public static PropertyDeclarationSyntax BuildPropertyWithSynchronizedBackingField(PropertyDeclarationSyntax property, BaseFieldDeclarationSyntax backingField)
        {
            var synchronizedProperty  =
                SyntaxFactory.PropertyDeclaration(property.Type, property.Identifier.Text)
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword).WithTrailingTrivia(SyntaxTriviaList.Create(SyntaxFactory.Space)))
                    .AddAccessorListAccessors(GetDefaultGetter(backingField), GetDefaultSetter(backingField)
                    );
            return synchronizedProperty;
        }
    }
}
