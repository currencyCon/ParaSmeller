using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConcurrencyChecker.HalfSynchronizedChecker
{
    public class HalfSynchronizedClassRepresentation
    {
        public HalfSynchronizedClassRepresentation(SyntaxNode classDeclaration)
        {
            Properties = classDeclaration.DescendantNodes().OfType<PropertyDeclarationSyntax>().ToList();
            Methods = classDeclaration.DescendantNodes().OfType<MethodDeclarationSyntax>().ToList();
            SynchronizedProperties = SyntaxNodeFilter.GetSynchronizedProperties(Properties);
            SynchronizedMethods = SyntaxNodeFilter.GetSynchronizedMethods(Methods);
            UnsynchronizedProperties = SyntaxNodeFilter.GetUnsynchronizedProperties(Properties);
            UnsynchronizedMethods = SyntaxNodeFilter.GetUnsynchronizedMethods(Methods);
            UnsynchronizedPropertiesInSynchronizedMethods =
                SyntaxNodeFilter.GetPropertiesInSynchronizedMethods(SynchronizedMethods, UnsynchronizedProperties);
        }
        public IEnumerable<PropertyDeclarationSyntax> Properties { get; set; }
        public IEnumerable<MethodDeclarationSyntax> Methods { get; set; }

        public IEnumerable<PropertyDeclarationSyntax> SynchronizedProperties { get; set; }
        public IEnumerable<MethodDeclarationSyntax> SynchronizedMethods { get; set; }
        public IEnumerable<PropertyDeclarationSyntax> UnsynchronizedProperties { get; set; }
        public IEnumerable<MethodDeclarationSyntax> UnsynchronizedMethods { get; set; }
        public IEnumerable<PropertyDeclarationSyntax> UnsynchronizedPropertiesInSynchronizedMethods { get; set; }

        public IEnumerable<string> GetIdentifiersInLockStatements()
        {
            return SyntaxNodeFilter.GetIdentifiersInLockStatements(SynchronizedProperties)
                .Concat(SyntaxNodeFilter.GetIdentifiersInLockStatements(SynchronizedMethods));
        }
    }
}
