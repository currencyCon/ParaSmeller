using System.Collections.Generic;
using System.Linq;
using ConcurrencyAnalyzer.Representation;
using ConcurrencyAnalyzer.RepresentationExtensions;
using ConcurrencyAnalyzer.SyntaxNodeUtils;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConcurrencyAnalyzer.SemanticAnalysis
{
    public static class SynchronizationInspector
    {
        public static IEnumerable<MethodRepresentation> GetMethodsWithHalfSynchronizedProperties(ClassRepresentation classRepresentation)
        {
            var methodsWithHalfSynchronizedProperties = new List<MethodRepresentation>();
            var unsyncedProperties = classRepresentation.UnSynchronizedProperties.Select(e => e.Implementation);
            var identifiersInSyncedMethods = classRepresentation.SynchronizedMethods.SelectMany(e => SyntaxNodeFilter.GetIdentifiersInLocks(e.Blocks)).Select(e => e.Identifier.Text);
            var unsProp = unsyncedProperties.Where(e => identifiersInSyncedMethods.Contains(e.Identifier.Text)).ToList();
            foreach (var unsyncedMethod in classRepresentation.UnSynchronizedMethods)
            {
                var identifiersInMethods = unsyncedMethod.GetChildren<IdentifierNameSyntax>().Select(e => e.Identifier.Text);
                if (unsProp.Select(e => e.Identifier.Text).Any(e => identifiersInMethods.Contains(e)))
                {
                    methodsWithHalfSynchronizedProperties.Add(unsyncedMethod);
                }
            }
            return methodsWithHalfSynchronizedProperties;
        }

        public static bool PropertyNeedsSynchronization(PropertyRepresentation propertyDeclaration, ClassRepresentation classRepresentation)
        {
            if (propertyDeclaration.IsFullySynchronized())
            {
                return false;
            }
            var identifiersInLockStatements = classRepresentation.GetIdentifiersInLocks().Select(e => e.Identifier.ToString());
            return identifiersInLockStatements.Contains(propertyDeclaration.Implementation.Identifier.Text);
            
        }

        public static bool MethodHasHalfSynchronizedProperties(MethodDeclarationSyntax method, ClassRepresentation classRepresentation)
        {
            var methodsWithHalfSynchronizedProperties = GetMethodsWithHalfSynchronizedProperties(classRepresentation);
            return methodsWithHalfSynchronizedProperties.Select(e => e.Implementation.Identifier.Text).Contains(method.Identifier.Text);
        }
    }
}