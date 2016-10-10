using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConcurrencyChecker.HalfSynchronizedChecker
{
    public class SynchronizationInspector
    {
        public static IEnumerable<MethodDeclarationSyntax> GetMethodsWithHalfSynchronizedProperties(HalfSynchronizedClassRepresentation halfSynchronizedClass)
        {
            var methodsWithHalfSynchronizedProperties = new List<MethodDeclarationSyntax>().ToList();
            foreach (var methodDeclarationSyntax in halfSynchronizedClass.UnsynchronizedMethods)
            {
                var identifiersInMethods =
                    methodDeclarationSyntax.DescendantNodesAndSelf()
                        .OfType<IdentifierNameSyntax>()
                        .Select(e => e.Identifier.Text);
                if (
                    halfSynchronizedClass.UnsynchronizedPropertiesInSynchronizedMethods.ToList()
                        .Select(e => e.Identifier.Text)
                        .Any(e => identifiersInMethods.Contains(e)))
                {
                    methodsWithHalfSynchronizedProperties.Add(methodDeclarationSyntax);
                }
            }
            return methodsWithHalfSynchronizedProperties;
        }

        public static PropertyDeclarationSyntax GetHalSynchronizedPropertyUsed(
            HalfSynchronizedClassRepresentation halfSynchronizedClass,
            MethodDeclarationSyntax methodWithHalfSynchronizedProperties)
        {
            var identifiersInMethods =
                methodWithHalfSynchronizedProperties.DescendantNodesAndSelf()
                    .OfType<IdentifierNameSyntax>()
                    .Select(e => e.Identifier.Text);

            var propUsed =
                halfSynchronizedClass.UnsynchronizedPropertiesInSynchronizedMethods.ToList()
                    .First(e => identifiersInMethods.Contains(e.Identifier.Text));
            return propUsed;
        }

        public static bool PropertyNeedsSynchronization(PropertyDeclarationSyntax propertyDeclaration, HalfSynchronizedClassRepresentation halfSynchronizedClass)
        {
            if (PropertyIsSynchronized(propertyDeclaration))
            {
                return false;
            }
            var identifiersInLockStatements = halfSynchronizedClass.GetIdentifiersInLockStatements();
            return identifiersInLockStatements.Contains(propertyDeclaration.Identifier.Text);
            
        }

        private static bool PropertyIsSynchronized(BasePropertyDeclarationSyntax propertyDeclaration)
        {
            var hasNullBodies = propertyDeclaration.AccessorList.Accessors.Any(e => e.Body == null);
            if (hasNullBodies)
            {
                return false;
            }
            return AccessorsAreSynchronized(propertyDeclaration);
        }

        private static bool AccessorsAreSynchronized(BasePropertyDeclarationSyntax propertyDeclaration)
        {
            var allAccessorsSynchronized = true;
            foreach (var accessorDeclarationSyntax in propertyDeclaration.AccessorList.Accessors)
            {
                var childrenOfAccessor = accessorDeclarationSyntax.Body.ChildNodes().ToList();
                if (childrenOfAccessor.Count > 1 || !(childrenOfAccessor.FirstOrDefault() is LockStatementSyntax))
                {
                    allAccessorsSynchronized = false;
                }
            }
            return allAccessorsSynchronized;
        }

        public static bool MethodHasHalfSynchronizedProperties(MethodDeclarationSyntax method, HalfSynchronizedClassRepresentation halfSynchronizedClass)
        {
            var methodsWithHalfSynchronizedProperties = GetMethodsWithHalfSynchronizedProperties(halfSynchronizedClass);
            return methodsWithHalfSynchronizedProperties.Select(e => e.Identifier.Text).Contains(method.Identifier.Text);
        }
    }
}