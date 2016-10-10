
using System.Linq;
using ConcurrencyAnalyzer.Representation;
using ConcurrencyAnalyzer.SyntaxFilters;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConcurrencyAnalyzer.RepresentationFactories
{
    public static class PropertyRepresentationFactory
    {
        public static IPropertyRepresentation Create(PropertyDeclarationSyntax propertyDeclarationSyntax, ClassRepresentation classRepresentation)
        {
            var isSynchronized = SyntaxNodeFilter.GetLockStatements(propertyDeclarationSyntax.AccessorList.Accessors.Select(e => e.Body)).Any();
            if (isSynchronized)
            {
                return CreateSynchronizedProperty(propertyDeclarationSyntax, classRepresentation);
            }
            return CreateUnsychronizedProperty(propertyDeclarationSyntax, classRepresentation);
        }

        private static IPropertyRepresentation CreateUnsychronizedProperty(PropertyDeclarationSyntax propertyDeclarationSyntax, ClassRepresentation classRepresentation)
        {
            var propertyRepresentation = new UnsynchronizedPropertyRepresentation(propertyDeclarationSyntax, classRepresentation);
            BuildInvocationExpressions(propertyRepresentation);
            return propertyRepresentation;
        }

        private static IPropertyRepresentation CreateSynchronizedProperty(PropertyDeclarationSyntax propertyDeclarationSyntax, ClassRepresentation classRepresentation)
        {
            var propertyRepresentation = new SynchronizedPropertyRepresentation(propertyDeclarationSyntax, classRepresentation);
            BuildInvocationExpressions(propertyRepresentation);
            return propertyRepresentation;
        }

        private static void BuildInvocationExpressions(IPropertyRepresentation propertyRepresentation)
        {
            var invocationExpressions = propertyRepresentation.Getter.GetChildren<InvocationExpressionSyntax>().Concat(propertyRepresentation.Setter.GetChildren<InvocationExpressionSyntax>());
            foreach (var invocationExpressionSyntax in invocationExpressions)
            {
                //propertyRepresentation.InvocationExpressions.Add(InvocationExpressionRepresentationFactory.Create(invocationExpressionSyntax, propertyRepresentation.ContainingClass, propertyRepresentation));
            }
        }
    }
}
