using ConcurrencyAnalyzer.Representation;
using ConcurrencyChecker.HalfSynchronizedChecker;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Diagnostic = ConcurrencyAnalyzer.Diagnostics.Diagnostic;

namespace ConcurrencyAnalyzer.Reporters.HalfSynchronizedReporter
{
    public class HalfSynchronizedReporter: BaseReporter
    {
        public const string HalfSynchronizedChildDiagnosticId = "HSC001";
        public const string UnsynchronizedPropertyId = "HSC002";

        public static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.HalfSynchronizedTitle), Resources.ResourceManager, typeof(Resources));
        public static readonly LocalizableString MessageFormatHalfSynchronized = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormatHalfSynchronized), Resources.ResourceManager, typeof(Resources));
        public static readonly LocalizableString MessageFormatUnsychronizedProperty = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormatUnsynchronizedProperty), Resources.ResourceManager, typeof(Resources));
        public static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.HalfSynchronizedDescription), Resources.ResourceManager, typeof(Resources));
        public const string Category = "Synchronization";

        private void DiagnoseMethod(MethodRepresentation method)
        {
            if (SynchronizationInspector.MethodHasHalfSynchronizedProperties(method.Implementation, method.ContainingClass))
            {
                Reports.Add(ReportHalfSynchronizationDiagnostic(method.Implementation, "Property", ""));
            }
        }

        private void DiagnoseProperty(PropertyRepresentation property)
        {
            if (SynchronizationInspector.PropertyNeedsSynchronization(property, property.ContainingClass))
            {
                Reports.Add(ReportUnsynchronizationPropertyDiagnostic(property.Implementation));
            }
        }

        private static Diagnostic ReportUnsynchronizationPropertyDiagnostic(CSharpSyntaxNode propertyDeclarationSyntax)
        {
            return new Diagnostic(UnsynchronizedPropertyId, Title, MessageFormatUnsychronizedProperty, Description,
                Category, propertyDeclarationSyntax.GetLocation());
        }

        private static Diagnostic ReportHalfSynchronizationDiagnostic(CSharpSyntaxNode propertyDeclarationSyntax, string elementType, string elementTypeName)
        {
            object[] messageArguments = { elementType, elementTypeName };
            return new Diagnostic(HalfSynchronizedChildDiagnosticId, Title, MessageFormatHalfSynchronized, Description, Category, propertyDeclarationSyntax.GetLocation(), messageArguments);
        }
        public override void Register()
        {
            RegisterMethodReport(DiagnoseMethod);
            RegisterPropertyReport(DiagnoseProperty);
        }
    }
}
