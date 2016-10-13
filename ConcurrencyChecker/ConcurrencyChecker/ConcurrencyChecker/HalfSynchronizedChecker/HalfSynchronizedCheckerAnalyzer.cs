using System.Collections.Immutable;
using System.Linq;
using ConcurrencyAnalyzer.Representation;
using ConcurrencyAnalyzer.RepresentationExtensions;
using ConcurrencyAnalyzer.RepresentationFactories;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ConcurrencyChecker.HalfSynchronizedChecker
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class HalfSynchronizedCheckerAnalyzer : DiagnosticAnalyzer
    {
        public const string HalfSynchronizedChildDiagnosticId = "HSC001";
        public const string UnsynchronizedPropertyId = "HSC002";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormatHalfSynchronized = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormatHalfSynchronized), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormatUnsychronizedProperty = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormatUnsynchronizedProperty), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Synchronization";

        private static readonly DiagnosticDescriptor RuleHalfSynchronized = new DiagnosticDescriptor(HalfSynchronizedChildDiagnosticId, Title, MessageFormatHalfSynchronized, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);
        private static readonly DiagnosticDescriptor RuleUnsynchronizedProperty = new DiagnosticDescriptor(UnsynchronizedPropertyId, Title, MessageFormatUnsychronizedProperty, Category, DiagnosticSeverity.Warning, isEnabledByDefault:true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(RuleHalfSynchronized, RuleUnsynchronizedProperty);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeForHalfSynchronizedProperties, SyntaxKind.PropertyDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeForHalfSynchronizedProperties, SyntaxKind.MethodDeclaration);
        }

        private static void AnalyzeForHalfSynchronizedProperties(SyntaxNodeAnalysisContext context)
        {
            var root = context.Node;

            var classDeclaration = root.FirstAncestorOrSelf<ClassDeclarationSyntax>();
            if (classDeclaration == null)
            {
                return;
            }
            var classRep = ClassRepresentationFactory.Create(classDeclaration, context.SemanticModel);


            if (!classRep.ClassHasSynchronizedMember())
            {
                return;
            }

            if (!classRep.Members.Any())
            {
                return;
            }
            if (root is PropertyDeclarationSyntax)
            {
                var prop = (PropertyDeclarationSyntax) root;
                var propInTree = classRep.GetMemberByName(prop.Identifier.ToString());
                DiagnoseProperty(context, (PropertyRepresentation)propInTree, classRep);
            }
            else if (root is MethodDeclarationSyntax)
            {
                DiagnoseMethod(context, (MethodDeclarationSyntax)root, classRep);
            }
        }

        private static void DiagnoseMethod(SyntaxNodeAnalysisContext context, MethodDeclarationSyntax method, ClassRepresentation classRepresentation)
        {
            if (SynchronizationInspector.MethodHasHalfSynchronizedProperties(method, classRepresentation))
            {
                ReportHalfSynchronizationDiagnostic(context, method, "Property", "");
            }
        }

        private static void DiagnoseProperty(SyntaxNodeAnalysisContext context, PropertyRepresentation property, ClassRepresentation classRepresentation)
        {
            if (SynchronizationInspector.PropertyNeedsSynchronization(property, classRepresentation))
            {
                ReportUnsynchronizationPropertyDiagnostic(context, property.PropertyImplementation);
            }
        }

        private static void ReportUnsynchronizationPropertyDiagnostic(SyntaxNodeAnalysisContext context,
    CSharpSyntaxNode propertyDeclarationSyntax)
        {
            var diagnostic = Diagnostic.Create(RuleUnsynchronizedProperty, propertyDeclarationSyntax.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }

        private static void ReportHalfSynchronizationDiagnostic(SyntaxNodeAnalysisContext context,
    CSharpSyntaxNode propertyDeclarationSyntax, string elementType, string elementTypeName)
        {

            object[] messageArguments = { elementType, elementTypeName };
            var diagnostic = Diagnostic.Create(RuleHalfSynchronized, propertyDeclarationSyntax.GetLocation(), messageArguments);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
