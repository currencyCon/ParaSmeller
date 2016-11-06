using System.Collections.Generic;
using ConcurrencyAnalyzer.Representation;
using ConcurrencyChecker.HalfSynchronizedChecker;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Diagnostic = ConcurrencyAnalyzer.Diagnostics.Diagnostic;

namespace ConcurrencyAnalyzer.Reporters.HalfSynchronizedReporter
{
    public class HalfSynchronizedReporter: IReporter
    {
        public const string HalfSynchronizedChildDiagnosticId = "HSC001";
        public const string UnsynchronizedPropertyId = "HSC002";

        public static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        public static readonly LocalizableString MessageFormatHalfSynchronized = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormatHalfSynchronized), Resources.ResourceManager, typeof(Resources));
        public static readonly LocalizableString MessageFormatUnsychronizedProperty = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormatUnsynchronizedProperty), Resources.ResourceManager, typeof(Resources));
        public static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        public const string Category = "Synchronization";

        private static void DiagnoseMethod(MethodDeclarationSyntax method, ClassRepresentation classRepresentation, ICollection<Diagnostic> reports)
        {
            if (SynchronizationInspector.MethodHasHalfSynchronizedProperties(method, classRepresentation))
            {
                reports.Add(ReportHalfSynchronizationDiagnostic(method, "Property", ""));
            }
        }

        private static void DiagnoseProperty(PropertyRepresentation property, ClassRepresentation classRepresentation, ICollection<Diagnostic> reports)
        {
            if (SynchronizationInspector.PropertyNeedsSynchronization(property, classRepresentation))
            {
                reports.Add(ReportUnsynchronizationPropertyDiagnostic(property.Implementation));
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
        public ICollection<Diagnostic> Report(SolutionRepresentation solutionRepresentation)
        {
            var reports = new List<Diagnostic>();
            foreach (var clazz in solutionRepresentation.Classes)
            {
                foreach (var methodRepresentation in clazz.Methods)
                {
                    DiagnoseMethod(methodRepresentation.Implementation, clazz, reports);
                }
                foreach (var propertyRepresentation in clazz.Properties)
                {
                    DiagnoseProperty(propertyRepresentation, clazz, reports);
                }
            }
            return reports;
        }
    }
}
