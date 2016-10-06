using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ExplicitThreadsChecker
{
    internal class RemoveUnusedUsings
    {

        public static async Task<Document> RemoveUnusedImportDirectivesAsync(Document document, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);

            root = RemoveUnusedImportDirectives(semanticModel, root, cancellationToken);
            document = document.WithSyntaxRoot(root);
            return document;
        }

        public static SyntaxNode RemoveUnusedImportDirectives(SemanticModel semanticModel, SyntaxNode root, CancellationToken cancellationToken)
        {
            var oldUsings = root.DescendantNodesAndSelf().Where(s => s is UsingDirectiveSyntax);
            var unusedUsings = GetUnusedImportDirectives(semanticModel, cancellationToken);
            var leadingTrivia = root.GetLeadingTrivia();

            root = root.RemoveNodes(oldUsings, SyntaxRemoveOptions.KeepNoTrivia);
            var newUsings = SyntaxFactory.List(oldUsings.Except(unusedUsings));

            root = ((CompilationUnitSyntax)root)
                .WithUsings(newUsings)
                .WithLeadingTrivia(leadingTrivia);

            return root;
        }

        private static HashSet<SyntaxNode> GetUnusedImportDirectives(SemanticModel model, CancellationToken cancellationToken)
        {
            var unusedImportDirectives = new HashSet<SyntaxNode>();
            var root = model.SyntaxTree.GetRoot(cancellationToken);
            foreach (var diagnostic in model.GetDiagnostics(null, cancellationToken).Where(d => d.Id == "CS8019" || d.Id == "CS0105"))
            {
                var usingDirectiveSyntax = root.FindNode(diagnostic.Location.SourceSpan, false, false) as UsingDirectiveSyntax;
                if (usingDirectiveSyntax != null)
                {
                    unusedImportDirectives.Add(usingDirectiveSyntax);
                }
            }

            return unusedImportDirectives;
        }

    }
}
