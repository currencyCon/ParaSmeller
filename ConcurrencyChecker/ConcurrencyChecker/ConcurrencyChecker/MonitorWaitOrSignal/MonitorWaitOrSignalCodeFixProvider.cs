using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ConcurrencyAnalyzer.SyntaxFilters;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConcurrencyChecker.MonitorWaitOrSignal
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MonitorWaitOrSignalCodeFixProvider)), Shared]
    public class MonitorWaitOrSignalCodeFixProvider : CodeFixProvider
    {
        private const string TitleIf = "Use if instead of while";
        private const string TitlePulse = "Use PulseAll instead of PUlse";
        private const string MonitorPulseAllMethod = "PulseAll";
        private const string MonitorPulseMethod = "Pulse";
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(MonitorWaitOrSignalAnalyzer.MonitorIfConditionDiagnosticId, MonitorWaitOrSignalAnalyzer.MonitorPulseDiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var diagnostic = context.Diagnostics.First();
            var node = root.FindNode(context.Span);

            if (node is InvocationExpressionSyntax)
            {
                context.RegisterCodeFix(CodeAction.Create(TitlePulse, c => ReplacePulseWithPulseAll(context.Document, (InvocationExpressionSyntax) node, c), TitlePulse), diagnostic);
            }
            else if (node is IfStatementSyntax)
            {
                context.RegisterCodeFix(CodeAction.Create(TitleIf, c => ReplaceIfWithWhile(context.Document, (IfStatementSyntax) node, c), TitleIf), diagnostic);
            }
        }

        private static async Task<Document> ReplaceIfWithWhile(Document document, IfStatementSyntax node, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var whileLoop = SyntaxFactory.WhileStatement(node.Condition, node.Statement);
            var newRoot = root.ReplaceNode(node, whileLoop);
            var newDocument = document.WithSyntaxRoot(newRoot);

            return newDocument;
        }

        private async Task<Document> ReplacePulseWithPulseAll(Document document, SyntaxNode node, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var identifier = node.GetChildren<IdentifierNameSyntax>().First(e => e.Identifier.ToString() == MonitorPulseMethod);
            var pulseAll = SyntaxFactory.IdentifierName(MonitorPulseAllMethod);
            var newRoot = root.ReplaceNode(identifier, pulseAll);
            var newDocument = document.WithSyntaxRoot(newRoot);

            return newDocument;
        }
    }
}