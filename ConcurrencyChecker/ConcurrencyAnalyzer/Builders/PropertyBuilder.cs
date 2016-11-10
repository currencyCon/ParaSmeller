using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConcurrencyAnalyzer.Builders
{
    public static class PropertyBuilder
    {
        private const string ValueKeyWord = "value";
        private const string BackingFieldPrefix = "_";

        public static FieldDeclarationSyntax BuildBackingField(PropertyDeclarationSyntax property)
        {
            var fieldName = BackingFieldPrefix + property.Identifier.Text;
            var variableDeclaration = SyntaxFactory.VariableDeclaration(property.Type, SyntaxFactory.SeparatedList(new[] { SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(fieldName)) }));
            var backingField = SyntaxFactory.FieldDeclaration(variableDeclaration).AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword).WithTrailingTrivia(SyntaxTriviaList.Create(SyntaxFactory.Space)));
            return backingField.WithLeadingTrivia(property.GetLeadingTrivia());
        }

        public static PropertyDeclarationSyntax BuildPropertyWithSynchronizedBackingField(PropertyDeclarationSyntax property, BaseFieldDeclarationSyntax backingField, ExpressionSyntax lockObject)
        {
            var synchronizedProperty  =
                SyntaxFactory.PropertyDeclaration(property.Type, property.Identifier.Text)
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword).WithTrailingTrivia(SyntaxTriviaList.Create(SyntaxFactory.Space)))
                    .AddAccessorListAccessors(DefaultGetter(backingField, lockObject), DefaultSetter(backingField, lockObject)
                    );
            return synchronizedProperty;
        }
        
        private static AccessorDeclarationSyntax DefaultSetter(BaseFieldDeclarationSyntax backingField, ExpressionSyntax lockObject)
        {
            var indention = SyntaxFactory.IdentifierName(backingField.Declaration.Variables.First().Identifier.ToString());
            var assignment = SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, indention,
                SyntaxFactory.IdentifierName(ValueKeyWord));
            var expressionStatement = SyntaxFactory.ExpressionStatement(assignment);
            var block = SyntaxFactory.Block(expressionStatement);
            var setter = SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration, LockBuilder.BuildLockBlock(block, lockObject));
            return setter;
        }

        private static AccessorDeclarationSyntax DefaultGetter(BaseFieldDeclarationSyntax backingField, ExpressionSyntax lockObject)
        {
            var body = SyntaxFactory.Block(
                SyntaxFactory.List(new[]
                {
                    SyntaxFactory.ReturnStatement(
                        SyntaxFactory.IdentifierName(backingField.Declaration.Variables.First().Identifier)
                            .WithLeadingTrivia(SyntaxTriviaList.Create(SyntaxFactory.Space)))
                })
                );
            var getter = SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration, LockBuilder.BuildLockBlock(body, lockObject));
            return getter;
        }
    }
}
