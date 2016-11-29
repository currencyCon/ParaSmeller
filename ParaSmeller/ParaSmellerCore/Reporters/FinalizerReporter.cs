using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ParaSmellerCore.Diagnostics;
using ParaSmellerCore.Representation;
using ParaSmellerCore.SyntaxNodeUtils;
using Diagnostic = ParaSmellerCore.Diagnostics.Diagnostic;

namespace ParaSmellerCore.Reporters
{
    public class FinalizerReporter : BaseReporter
    {
        public const string Category = "Synchronization";
        public const string FinalizerSynchronizationDiagnosticId = "FS001";

        public static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.FSAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        public static readonly LocalizableString MessageFormatFinalizerSynchronization = new LocalizableResourceString(nameof(Resources.FinalizerSynchronizationAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        public static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.FSAnalyzerDescription), Resources.ResourceManager, typeof(Resources));

        private void CheckClassForUnsynchronizedFinalizers(ClassRepresentation classRepresentation)
        {
            if (classRepresentation.Destructor == null)
            {
                return;
            }
            CheckForUnsynchronizedFields(classRepresentation);
            CheckForUnsynchronizedProperties(classRepresentation);
            CheckIfAllSynchronizedFieldsUseSameStaticLock(classRepresentation);
        }

        private void CheckIfAllSynchronizedFieldsUseSameStaticLock(ClassRepresentation classRepresentation)
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
                        Reports.Add(ReportUnsynchronizedField(field));
                        Reports.Add(ReportUnsynchronizedField(fieldUsedInDestructor));
                    }
                    if (!classRepresentation.IsStaticDefinedLockObject(fieldDeclarationLock))
                    {
                        Reports.Add(ReportUnsynchronizedField(field));
                    }
                    if (!destructorLocks.Any(classRepresentation.IsStaticDefinedLockObject))
                    {
                        Reports.Add(ReportUnsynchronizedField(fieldUsedInDestructor));
                    }
                }
            }
        }

        private void CheckForUnsynchronizedProperties(ClassRepresentation classRepresentation)
        {
            var membersUsedInDestructor = classRepresentation.Destructor.GetChildren<IdentifierNameSyntax>().ToList();
            foreach (var memberUsedInDestructor in membersUsedInDestructor)
            {
                foreach (var unsynchronizedProperty in classRepresentation.UnSynchronizedProperties.Select(e => e.Implementation))
                {
                    if (unsynchronizedProperty.Identifier.Text == memberUsedInDestructor.Identifier.ToString())
                    {
                        Reports.Add(ReportUnsynchronizedField(unsynchronizedProperty));
                        if (!memberUsedInDestructor.GetParents<LockStatementSyntax>().Any())
                        {
                            Reports.Add(ReportUnsynchronizedField(memberUsedInDestructor));
                        }
                    }
                }
            }
        }

        private void CheckForUnsynchronizedFields(ClassRepresentation classRepresentation)
        {
            var fieldsUsedInDestructorUnsynchronized = classRepresentation.Destructor.GetChildren<IdentifierNameSyntax>().Where(e => !e.GetParents<LockStatementSyntax>().Any()).ToList();
            foreach (var fieldUsedInDestructor in fieldsUsedInDestructorUnsynchronized)
            {
                foreach (var field in classRepresentation.Fields.With(new[] { "static" }).Without(new[] { "readonly" }).ToList())
                {
                    if (field.DeclaresVariable(fieldUsedInDestructor.Identifier.Text))
                    {
                        Reports.Add(ReportUnsynchronizedField(field));
                        Reports.Add(ReportUnsynchronizedField(fieldUsedInDestructor));
                    }
                }
            }
        }

        private static Diagnostic ReportUnsynchronizedField(SyntaxNode syntaxnode)
        {
            return new Diagnostic(FinalizerSynchronizationDiagnosticId, Title, MessageFormatFinalizerSynchronization, Description, Category, syntaxnode.GetLocation());
        }

        protected override void Register()
        {
            RegisterClassReport(CheckClassForUnsynchronizedFinalizers);
        }
    }
}
