using System.Collections.Generic;
using System.Linq;
using ConcurrencyAnalyzer.Representation;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConcurrencyChecker.HalfSynchronizedChecker
{
    public class SynchronizationInspector
    {
        public static IEnumerable<MethodDeclarationSyntax> GetMethodsWithHalfSynchronizedProperties(ClassRepresentation classRepresentation)
        {
            var methodsWithHalfSynchronizedProperties = new List<MethodDeclarationSyntax>().ToList();
            var uns =
                classRepresentation.Members.Where(e => e is MethodRepresentation && !e.IsFullySynchronized())
                    .Select(e => e as MethodRepresentation)
                    .Select(e => e.MethodImplementation);
            var syn = classRepresentation.Members.Where(e => e is MethodRepresentation && e.IsFullySynchronized())
                    .Select(e => e as MethodRepresentation);
            var props = classRepresentation.Members.Where(e => e is PropertyRepresentation && !e.IsFullySynchronized())
                    .Select(e => e as PropertyRepresentation)
                    .Select(e => e.PropertyImplementation);
            var x =
                syn.SelectMany(e => ConcurrencyAnalyzer.SyntaxFilters.SyntaxNodeFilter.GetIdentifiersInLocks(e.Blocks)).Select(e => e.Identifier.Text);
            var unsProp = props.Where(e => x.Contains(e.Identifier.Text)).ToList();
            foreach (var methodDeclarationSyntax in uns)
            {
                var identifiersInMethods =
                    methodDeclarationSyntax.DescendantNodesAndSelf()
                        .OfType<IdentifierNameSyntax>()
                        .Select(e => e.Identifier.Text);
                if (
                    unsProp
                        .Select(e => e.Identifier.Text)
                        .Any(e => identifiersInMethods.Contains(e)))
                {
                    methodsWithHalfSynchronizedProperties.Add(methodDeclarationSyntax);
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

            var identifiersInLockStatements =
                classRepresentation.GetIdentifiersInLocks().Select(e => e.Identifier.ToString());
            return identifiersInLockStatements.Contains(propertyDeclaration.PropertyImplementation.Identifier.Text);
            
        }

        public static bool MethodHasHalfSynchronizedProperties(MethodDeclarationSyntax method, ClassRepresentation classRepresentation)
        {
            var methodsWithHalfSynchronizedProperties = GetMethodsWithHalfSynchronizedProperties(classRepresentation);
            return methodsWithHalfSynchronizedProperties.Select(e => e.Identifier.Text).Contains(method.Identifier.Text);
        }
    }
}