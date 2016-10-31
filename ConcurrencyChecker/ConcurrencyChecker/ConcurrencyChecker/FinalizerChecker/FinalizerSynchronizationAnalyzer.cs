
using System.Collections.Immutable;
using System.Linq;
using ConcurrencyAnalyzer.Representation;
using ConcurrencyAnalyzer.RepresentationFactories;
using ConcurrencyAnalyzer.SyntaxFilters;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ConcurrencyChecker.FinalizerChecker
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class FinalizerSynchronizationAnalyzer: DiagnosticAnalyzer
    {
        private const string Category = "Synchronization";
        public const string FinalizerSynchronizationDiagnosticId = "PS001";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.FSAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        public static readonly LocalizableString MessageFormatFinalizerSynchronization = new LocalizableResourceString(nameof(Resources.FinalizerSynchronizationAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));

        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.FSAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private static readonly DiagnosticDescriptor FinalizerSynchronizationUsageRule = new DiagnosticDescriptor(FinalizerSynchronizationDiagnosticId, Title, MessageFormatFinalizerSynchronization, Category, DiagnosticSeverity.Warning, true, Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(FinalizerSynchronizationUsageRule);
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationAction(CheckForUnsynchronizedFinalizers);
        }

        private static async void CheckForUnsynchronizedFinalizers(CompilationAnalysisContext context)
        {
            var solutionModel = await SolutionRepresentationFactory.Create(context.Compilation);
            foreach (var classRepresentation in solutionModel.Classes)
            {
                CheckClassForUnsynchronizedFinalizers(classRepresentation, context);
            }
        }

        private static void CheckClassForUnsynchronizedFinalizers(ClassRepresentation classRepresentation, CompilationAnalysisContext context)
        {
            if (classRepresentation.Destructor == null)
            {
                return;
            }
            CheckForUnsynchronizedFields(classRepresentation, context);
            CheckForUnsynchronizedProperties(classRepresentation, context);
            CheckIfAllSynchronizedFieldsUseSameStaticLock(classRepresentation, context);
        }

        private static void CheckIfAllSynchronizedFieldsUseSameStaticLock(ClassRepresentation classRepresentation, CompilationAnalysisContext context)
        {
            var fieldsUsedInDestructor = classRepresentation.Destructor.GetChildren<IdentifierNameSyntax>().Where(e => e.GetParents<LockStatementSyntax>().Any() && e.GetParents<ExpressionStatementSyntax>().Any()).ToList();
            var lockedFieldUsages = classRepresentation.Implementation.GetChildren<IdentifierNameSyntax>().Where(e => e.GetParents<LockStatementSyntax>().Any() && !(e.Parent is LockStatementSyntax)&& ! e.GetParents<DestructorDeclarationSyntax>().Any()).ToList();

            foreach (var fieldUsedInDestructor in fieldsUsedInDestructor)
            {
                foreach (var fieldDeclarationSyntax in lockedFieldUsages)
                {
                    var fieldDeclarationLock = fieldDeclarationSyntax.GetFirstParent<LockStatementSyntax>();
                    var destructorLocks = fieldUsedInDestructor.GetParents<LockStatementSyntax>().ToList();
                    if (
                        !destructorLocks.Select(e => e.Expression.ToString())
                            .Contains(fieldDeclarationLock.Expression.ToString()))
                    {
                        ReportUnsynchronizedField(context, fieldDeclarationSyntax);
                        ReportUnsynchronizedField(context, fieldUsedInDestructor);
                    }
                    if (!classRepresentation.IsStaticDefinedLockObject(fieldDeclarationLock))
                    {
                        ReportUnsynchronizedField(context, fieldDeclarationSyntax);

                    }
                    if (!destructorLocks.Any(classRepresentation.IsStaticDefinedLockObject))
                    {
                        ReportUnsynchronizedField(context, fieldUsedInDestructor);
                    }
                }
            }
        }

        private static void CheckForUnsynchronizedProperties(ClassRepresentation classRepresentation, CompilationAnalysisContext context)
        {
            var fieldsUsedInDestructor = classRepresentation.Destructor.GetChildren<IdentifierNameSyntax>().ToList();
            var fieldDeclarations = classRepresentation.UnSynchronizedProperties.Select(e => e.Implementation).ToList();
            foreach (var fieldUsedInDestructor in fieldsUsedInDestructor)
            {
                foreach (var fieldDeclarationSyntax in fieldDeclarations)
                {
                    if (
                        fieldDeclarationSyntax.Identifier.Text == fieldUsedInDestructor.Identifier.ToString())
                    {
                        ReportUnsynchronizedField(context, fieldDeclarationSyntax);
                        if (!fieldUsedInDestructor.GetParents<LockStatementSyntax>().Any())
                        {
                            ReportUnsynchronizedField(context, fieldUsedInDestructor);
                        }
                        
                    }
                }
            }
        }

        private static void CheckForUnsynchronizedFields(ClassRepresentation classRepresentation, CompilationAnalysisContext context)
        {
            var fieldsUsedInDestructor = classRepresentation.Destructor.GetChildren<IdentifierNameSyntax>().Where(e => !(e.Parent is LockStatementSyntax) && !e.GetParents<LockStatementSyntax>().Any()).ToList();
            if (!fieldsUsedInDestructor.Any())
            {
                return;
            }
            var fieldDeclarations = classRepresentation.Implementation.GetChildren<FieldDeclarationSyntax>().ToList();
            foreach (var fieldUsedInDestructor in fieldsUsedInDestructor)
            {
                foreach (var fieldDeclarationSyntax in fieldDeclarations)
                {
                    if (
                        fieldDeclarationSyntax.Declaration.Variables.Any(
                            e => e.Identifier.Text == fieldUsedInDestructor.Identifier.ToString()))
                    {
                        ReportUnsynchronizedField(context, fieldDeclarationSyntax);
                        ReportUnsynchronizedField(context, fieldUsedInDestructor);
                    }
                }
            }
        }

        private static void ReportUnsynchronizedField(CompilationAnalysisContext context, SyntaxNode syntaxnode)
        {
            var diagn = Diagnostic.Create(FinalizerSynchronizationUsageRule, syntaxnode.GetLocation());
            context.ReportDiagnostic(diagn);
        }
    }
}
