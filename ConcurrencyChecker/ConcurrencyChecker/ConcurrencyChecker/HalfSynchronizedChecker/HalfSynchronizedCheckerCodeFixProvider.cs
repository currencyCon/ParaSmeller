using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace ConcurrencyChecker.HalfSynchronizedChecker
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(HalfSynchronizedCheckerCodeFixProvider)), Shared]
    public class HalfSynchronizedCheckerCodeFixProvider : CodeFixProvider
    {
        private const string Title = "Synchronize Member";

        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(HalfSynchronizedCheckerAnalyzer.HalfSynchronizedChildDiagnosticId, HalfSynchronizedCheckerAnalyzer.UnsynchronizedPropertyId);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var syntaxNode = root.FindToken(diagnosticSpan.Start).Parent;
            if (syntaxNode is MethodDeclarationSyntax)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(Title, c => SynchronizeMethod(context.Document, (MethodDeclarationSyntax) syntaxNode, c), Title), context.Diagnostics.First(a => a.Id == HalfSynchronizedCheckerAnalyzer.HalfSynchronizedChildDiagnosticId));
            }
            else if (syntaxNode is PropertyDeclarationSyntax)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(Title, c => SynchronizeProperty(context.Document, (PropertyDeclarationSyntax)syntaxNode, c), equivalenceKey: Title), context.Diagnostics.First(a => a.Id == HalfSynchronizedCheckerAnalyzer.UnsynchronizedPropertyId));
            }
        }

        private static async Task<Document> SynchronizeProperty(Document document, PropertyDeclarationSyntax property,
    CancellationToken cancellationToken)
        {

            var backingField = PropertyBuilder.BuildBackingField(property);
            var newProperty = PropertyBuilder.BuildPropertyWithSynchronizedBackingField(property, backingField);
            var documentEditor = await DocumentEditor.CreateAsync(document, cancellationToken);
            documentEditor.InsertBefore(property, backingField);
            documentEditor.ReplaceNode(property, newProperty);
            return documentEditor.GetChangedDocument();         
        }

        private static async Task<Document> SynchronizeMethod(Document document, MethodDeclarationSyntax method,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var newMeth = BuildLockedMethod(method);
            return document.WithSyntaxRoot(root.ReplaceNode(method, newMeth));

        }

        private static MethodDeclarationSyntax BuildLockedMethod(MethodDeclarationSyntax method)
        {
            var body = method.Body;
            foreach (var statementSyntax in body.Statements)
            {
                body = body.ReplaceNode(statementSyntax, SyntaxFormatter.AddIndention(statementSyntax, 1));
            }
            body = body.ReplaceToken(body.CloseBraceToken, SyntaxFormatter.AddIndention(body.CloseBraceToken, 1));
            var lockStatementBlock = LockBuilder.BuildLockBlock(body);
            var newMeth = method.ReplaceNode(method, method.WithBody(lockStatementBlock));
            return newMeth;
        }
    }
}