using System.Collections.Generic;
using System.Linq;
using ConcurrencyAnalyzer.Representation;
using ConcurrencyAnalyzer.SyntaxNodeUtils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Diagnostic = ConcurrencyAnalyzer.Diagnostics.Diagnostic;

namespace ConcurrencyAnalyzer.Reporters.FinalizerReporter
{
    public class FinalizerReporter: IReporter
    {
        public const string Category = "Synchronization";
        public const string FinalizerSynchronizationDiagnosticId = "PS001";

        public static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.FSAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        public static readonly LocalizableString MessageFormatFinalizerSynchronization = new LocalizableResourceString(nameof(Resources.FinalizerSynchronizationAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));

        public static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.FSAnalyzerDescription), Resources.ResourceManager, typeof(Resources));

        private static void CheckClassForUnsynchronizedFinalizers(ClassRepresentation classRepresentation, ICollection<Diagnostic> reports)
        {
            if (classRepresentation.Destructor == null)
            {
                return;
            }
            CheckForUnsynchronizedFields(classRepresentation, reports);
            CheckForUnsynchronizedProperties(classRepresentation, reports);
            CheckIfAllSynchronizedFieldsUseSameStaticLock(classRepresentation, reports);
        }

        private static void CheckIfAllSynchronizedFieldsUseSameStaticLock(ClassRepresentation classRepresentation, ICollection<Diagnostic> reports)
        {
            var fieldsUsedInDestructorSynchronized = classRepresentation.Destructor.GetChildren<IdentifierNameSyntax>().
                Where(e => e.GetParents<LockStatementSyntax>().Any() &&
                e.GetParents<ExpressionStatementSyntax>().Any()).ToList();
            var lockedFieldUsagesOutsideDeconstructor = classRepresentation.Implementation.GetChildren<IdentifierNameSyntax>().
                Where(e => e.GetParents<LockStatementSyntax>().Any() &&
                !(e.Parent is LockStatementSyntax) &&
                !e.GetParents<DestructorDeclarationSyntax>().Any()).ToList();

            foreach (var fieldUsedInDestructor in fieldsUsedInDestructorSynchronized)
            {
                foreach (var field in lockedFieldUsagesOutsideDeconstructor)
                {
                    var fieldDeclarationLock = field.GetFirstParent<LockStatementSyntax>();
                    var destructorLocks = fieldUsedInDestructor.GetParents<LockStatementSyntax>().ToList();
                    if (!destructorLocks.Select(e => e.Expression.ToString()).Contains(fieldDeclarationLock.Expression.ToString()))
                    {
                        reports.Add(ReportUnsynchronizedField(field));
                        reports.Add(ReportUnsynchronizedField(fieldUsedInDestructor));
                    }
                    if (!classRepresentation.IsStaticDefinedLockObject(fieldDeclarationLock))
                    {
                        reports.Add(ReportUnsynchronizedField(field));
                    }
                    if (!destructorLocks.Any(classRepresentation.IsStaticDefinedLockObject))
                    {
                        reports.Add(ReportUnsynchronizedField(fieldUsedInDestructor));
                    }
                }
            }
        }

        private static void CheckForUnsynchronizedProperties(ClassRepresentation classRepresentation, ICollection<Diagnostic> reports)
        {
            var membersUsedInDestructor = classRepresentation.Destructor.GetChildren<IdentifierNameSyntax>().ToList();
            foreach (var memberUsedInDestructor in membersUsedInDestructor)
            {
                foreach (var unsynchronizedProperty in classRepresentation.UnSynchronizedProperties.Select(e => e.Implementation))
                {
                    if (unsynchronizedProperty.Identifier.Text == memberUsedInDestructor.Identifier.ToString())
                    {
                        reports.Add(ReportUnsynchronizedField(unsynchronizedProperty));
                        if (!memberUsedInDestructor.GetParents<LockStatementSyntax>().Any())
                        {
                            reports.Add(ReportUnsynchronizedField(memberUsedInDestructor));
                        }
                    }
                }
            }
        }

        private static void CheckForUnsynchronizedFields(ClassRepresentation classRepresentation, ICollection<Diagnostic> reports)
        {
            var fieldsUsedInDestructorUnsynchronized = classRepresentation.Destructor.GetChildren<IdentifierNameSyntax>().Where(e => !e.GetParents<LockStatementSyntax>().Any()).ToList();
            foreach (var fieldUsedInDestructor in fieldsUsedInDestructorUnsynchronized)
            {
                foreach (var field in classRepresentation.Fields.ToList())
                {
                    if (field.DeclaresVariable(fieldUsedInDestructor.Identifier.Text))
                    {
                        reports.Add(ReportUnsynchronizedField(field));
                        reports.Add(ReportUnsynchronizedField(fieldUsedInDestructor));
                    }
                }
            }
        }

        private static Diagnostic ReportUnsynchronizedField(SyntaxNode syntaxnode)
        {
            return new Diagnostic(FinalizerSynchronizationDiagnosticId, Title, MessageFormatFinalizerSynchronization, Description, Category, syntaxnode.GetLocation());

        }
        public ICollection<Diagnostic> Report(SolutionRepresentation solutionRepresentation)
        {
            var reports = new List<Diagnostic>();
            foreach (var clazz in solutionRepresentation.Classes)
            {
                CheckClassForUnsynchronizedFinalizers(clazz, reports);
            }
            return reports;
        }
    }
}
