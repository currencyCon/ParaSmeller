using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ExplicitThreadsChecker;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace ConcurrencyChecker.ExplicitThreadsChecker
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ExplicitThreadsMultilineCheckerCodeFixProvider)), Shared]
    public class ExplicitThreadsMultilineCheckerCodeFixProvider : CodeFixProvider
    {
        private const string Title = "Use Task.Run2";
        private const string TaskUsing = "System.Threading.Tasks";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
            => ImmutableArray.Create(ExplicitThreadsMultilineCheckerAnalyzer.DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();

            var node = root.FindNode(context.Span);

            if (node is MemberAccessExpressionSyntax == false)
            {
                return;
            }

            var mem = (MemberAccessExpressionSyntax) node;

            context.RegisterCodeFix(
                CodeAction.Create(Title, c => ReplaceThreadWithTask(context.Document, mem, c), Title),
                diagnostic);
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

            var invocationStatement = TaskSyntaxHelper.CreateInvocationStatement(threadArgument);

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
            var node = block.GetLocalDeclaredVariables().SingleVariable(variableName);
            var nodeToDelete = node.AncestorsAndSelf().OfType<LocalDeclarationStatementSyntax>().First();
            editor.RemoveNode(nodeToDelete);
        }

        private void RemoveVariableDeclartion(string variableName, SyntaxNode block, SyntaxEditor editor)
        {
            var declaredVariables = block.GetLocalDeclaredVariables()
                .SingleVariable(variableName)
                .AncestorsAndSelf()
                .OfType<LocalDeclarationStatementSyntax>().First();

            var variableNodeToRemove =
                declaredVariables
                    .DescendantNodes()
                    .OfType<VariableDeclaratorSyntax>().SingleVariable(variableName);

            var newDeclaration = declaredVariables.RemoveNode(variableNodeToRemove, SyntaxRemoveOptions.KeepEndOfLine);
            editor.ReplaceNode(declaredVariables, newDeclaration);
        }

        private void RemoveDirectInstantiation(string variableName, SyntaxNode block, SyntaxEditor editor)
        {
            var node = block.GetLocalDeclaredVariables().SingleVariable(variableName);
            var nodeToDelete = node.AncestorsAndSelf().OfType<LocalDeclarationStatementSyntax>().First();
            editor.RemoveNode(nodeToDelete);
        }

        private void RemoveSeparateInstantiation(string variableName, SyntaxNode block, SyntaxEditor editor)
        {
            var node = block
                .DescendantNodes()
                .OfType<AssignmentExpressionSyntax>()
                .First(a => a.Left.ToString() == variableName).Parent;
            var nodeToDelete = node.AncestorsAndSelf().OfType<ExpressionStatementSyntax>().First();
            editor.RemoveNode(nodeToDelete);
        }


        private bool IsOnlyDeclaredVariable(string variableName, SyntaxNode block)
        {
            return
                block.GetLocalDeclaredVariables()
                .SingleVariable(variableName).Parent
                    .DescendantNodes()
                    .OfType<VariableDeclaratorSyntax>()
                    .Count() == 1;
        }

        private bool IsThreadDeclaredSeparatly(string variableName, SyntaxNode block)
        {
            return !
                block.DescendantNodes()
                    .OfType<VariableDeclaratorSyntax>()
                    .Where(a => a.Identifier.ToString() == variableName)
                    .SelectMany(b => b.DescendantNodes().OfType<EqualsValueClauseSyntax>())
                    .Any();
        }

        private ArgumentSyntax FindThreadArgument(string variableName, BlockSyntax block)
        {
            var creationNodes = block.DescendantNodes().OfType<ObjectCreationExpressionSyntax>();
            foreach (var creation in creationNodes)
            {
                if (creation.AncestorsAndSelf().OfType<VariableDeclaratorSyntax>().Any())
                {
                    if (creation.AncestorsAndSelf().OfType<VariableDeclaratorSyntax>().First().Identifier.ToString() ==
                        variableName)
                    {
                        return creation.DescendantNodes().OfType<ArgumentSyntax>().First();
                    }
                }
                else if (creation.AncestorsAndSelf().OfType<AssignmentExpressionSyntax>().Any())
                {
                    if (creation.AncestorsAndSelf().OfType<AssignmentExpressionSyntax>().First().Left.ToString() ==
                        variableName)
                    {
                        return creation.DescendantNodes().OfType<ArgumentSyntax>().First();
                    }
                }
            }
            return null;
        }
    }
}