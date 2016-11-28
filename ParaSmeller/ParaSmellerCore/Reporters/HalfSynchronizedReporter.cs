using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using ParaSmellerCore.Diagnostics;
using ParaSmellerCore.Representation;
using Diagnostic = ParaSmellerCore.Diagnostics.Diagnostic;

namespace ParaSmellerCore.Reporters
{
    public class HalfSynchronizedReporter: BaseReporter
    {
        public const string HalfSynchronizedChildDiagnosticId = "HSC001";
        public const string UnsynchronizedPropertyId = "HSC002";

        public static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.HalfSynchronizedTitle), Resources.ResourceManager, typeof(Resources));
        public static readonly LocalizableString MessageFormatHalfSynchronized = new LocalizableResourceString(nameof(Resources.HalfSynchronizedAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        public static readonly LocalizableString MessageFormatUnsychronizedProperty = new LocalizableResourceString(nameof(Resources.UnsynchronizedPropertyAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        public static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.HalfSynchronizedDescription), Resources.ResourceManager, typeof(Resources));

        private void DiagnoseMethod(MethodRepresentation method)
        {
            if (method.MethodHasHalfSynchronizedProperties())
            {
                Reports.Add(ReportHalfSynchronizationDiagnostic(method.Implementation, "Property", ""));
            }
        }

        private void DiagnoseProperty(PropertyRepresentation property)
        {
            if (property.NeedsSynchronization())
            {
                Reports.Add(ReportUnsynchronizationPropertyDiagnostic(property.Implementation));
            }
        }

        private static Diagnostic ReportUnsynchronizationPropertyDiagnostic(CSharpSyntaxNode propertyDeclarationSyntax)
        {
            return new Diagnostic(UnsynchronizedPropertyId, Title, MessageFormatUnsychronizedProperty, Description,
                DiagnosticCategory.Synchronization, propertyDeclarationSyntax.GetLocation());
        }

        private static Diagnostic ReportHalfSynchronizationDiagnostic(CSharpSyntaxNode propertyDeclarationSyntax, string elementType, string elementTypeName)
        {
            object[] messageArguments = { elementType, elementTypeName };
            return new Diagnostic(HalfSynchronizedChildDiagnosticId, Title, MessageFormatHalfSynchronized, Description, DiagnosticCategory.Synchronization, propertyDeclarationSyntax.GetLocation(), messageArguments);
        }

        protected override void Register()
        {
            RegisterMethodReport(DiagnoseMethod);
            RegisterPropertyReport(DiagnoseProperty);
        }
    }
}
