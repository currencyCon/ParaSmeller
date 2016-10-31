using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ConcurrencyAnalyzer.Builders;
using ConcurrencyAnalyzer.SyntaxNodeUtils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace ConcurrencyChecker.ExplicitThreadsChecker
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ExplicitThreadsMultilineCodeFixProvider)), Shared]
    public class ExplicitThreadsMultilineCodeFixProvider : CodeFixProvider
    {
        private const string Title = "Use Task.Run2";
        private const string TaskUsing = "System.Threading.Tasks";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
            => ImmutableArray.Create(ExplicitThreadsMultilineAnalyzer.DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var diagnostic = context.Diagnostics.First();
            var node = root.FindNode(context.Span);

            if (!(node is MemberAccessExpressionSyntax))
            {
                return;
            }

            var mem = (MemberAccessExpressionSyntax) node;

            context.RegisterCodeFix(CodeAction.Create(Title, c => ReplaceThreadWithTask(context.Document, mem, c), Title), diagnostic);
        }

        private async Task<Document> ReplaceThreadWithTask(Document document, MemberAccessExpressionSyntax node,
            CancellationToken cancellationToken)
        {
            var block = node.Ancestors().OfType<BlockSyntax>().First();
            var variableName = node.Expression.ToString();

            var threadArgument = FindThreadArgument(variableName, block);
            if (threadArgument == null)
            {
                return document;
            }

            var invocationStatement = TaskSyntaxBuilder.CreateInvocationStatement(threadArgument);

            var documentEditor = await DocumentEditor.CreateAsync(document, cancellationToken);
            documentEditor.ReplaceNode(node.Parent, invocationStatement);

            RemoveThreadingCode(variableName, block, documentEditor);

            var newDocument = documentEditor.GetChangedDocument();
            newDocument = await AddUsingsToDocument(newDocument);

            return newDocument;
        }

        private static async Task<Document> AddUsingsToDocument(Document document)
        {
            var newSyntaxRoot = await document.GetSyntaxRootAsync();
            newSyntaxRoot = UsingHandler.AddUsingIfNotExists(newSyntaxRoot, TaskUsing);
            return document.WithSyntaxRoot(newSyntaxRoot);
        }

        private void RemoveThreadingCode(string variableName, BlockSyntax block, DocumentEditor documentEditor)
        {
            if (IsThreadDeclaredSeparatly(variableName, block))
            {
                if (IsOnlyDeclaredVariable(variableName, block))
                {
                    RemoveDeclaration(variableName, block, documentEditor);
                }
                else
                {
                    RemoveVariableDeclartion(variableName, block, documentEditor);
                }
                RemoveSeparateInstantiation(variableName, block, documentEditor);
            }
            else
            {
                RemoveDirectInstantiation(variableName, block, documentEditor);
            }
        }


        private void RemoveDeclaration(string variableName, BlockSyntax block, DocumentEditor editor)
        {
            var node = block.GetLocalDeclaredVariables().FindVariableDeclaration(variableName);
            var nodeToDelete = node.AncestorsAndSelf().OfType<LocalDeclarationStatementSyntax>().First();
            editor.RemoveNode(nodeToDelete);
        }

        private void RemoveVariableDeclartion(string variableName, SyntaxNode block, SyntaxEditor editor)
        {
            var declaredVariables = block.GetLocalDeclaredVariables()
                .FindVariableDeclaration(variableName).GetParents<LocalDeclarationStatementSyntax>().First();

            var variableNodeToRemove =
                declaredVariables.GetChildren<VariableDeclaratorSyntax>().FindVariableDeclaration(variableName);

            var newDeclaration = declaredVariables.RemoveNode(variableNodeToRemove, SyntaxRemoveOptions.KeepEndOfLine);
            editor.ReplaceNode(declaredVariables, newDeclaration);
        }

        private void RemoveDirectInstantiation(string variableName, SyntaxNode block, SyntaxEditor editor)
        {
            var node = block.GetLocalDeclaredVariables().FindVariableDeclaration(variableName);
            var nodeToDelete = node.GetParents<LocalDeclarationStatementSyntax>().First();
            editor.RemoveNode(nodeToDelete);
        }

        private void RemoveSeparateInstantiation(string variableName, SyntaxNode block, SyntaxEditor editor)
        {
            var node = block.GetChildren<AssignmentExpressionSyntax>()
                .First(a => a.Left.ToString() == variableName).Parent;
            var nodeToDelete = node.GetParents<ExpressionStatementSyntax>().First();
            editor.RemoveNode(nodeToDelete);
        }


        private bool IsOnlyDeclaredVariable(string variableName, SyntaxNode block)
        {
            return
                block.GetLocalDeclaredVariables()
                .FindVariableDeclaration(variableName).Parent.GetChildren<VariableDeclaratorSyntax>()
                    .Count() == 1;
        }

        private static bool IsThreadDeclaredSeparatly(string variableName, SyntaxNode block)
        {
            return !
                block.GetChildren<VariableDeclaratorSyntax>()
                    .Where(a => a.Identifier.ToString() == variableName)
                    .SelectMany(b => b.GetChildren<EqualsValueClauseSyntax>())
                    .Any();
        }

        private static ArgumentSyntax FindThreadArgument(string variableName, SyntaxNode block)
        {
            var creationNodes = block.GetChildren<ObjectCreationExpressionSyntax>();
            foreach (var creation in creationNodes)
            {
                if (creation.GetParents<VariableDeclaratorSyntax>().Any())
                {
                    if (creation.GetParents<VariableDeclaratorSyntax>().First().Identifier.ToString() ==
                        variableName)
                    {
                        return creation.GetChildren<ArgumentSyntax>().First();
                    }
                }
                else if (creation.AncestorsAndSelf().OfType<AssignmentExpressionSyntax>().Any(e => e.Left.ToString() == variableName))
                {
                    return creation.GetChildren<ArgumentSyntax>().First();
                }
            }
            return null;
        }
    }
}