using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ConcurrencyAnalyzer.Reporters.TapirReporter;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConcurrencyChecker.EasterEgg
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(TapirCodeFixProvider)), Shared]
    public class TapirCodeFixProvider : CodeFixProvider
    {
        private const string Title = "Add a tapir";

        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(TapirReporter.DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var diagnostic = context.Diagnostics.First();
            var node = root.FindNode(context.Span);

            if (!(node is ClassDeclarationSyntax))
            {
                return;
            }

            context.RegisterCodeFix(CodeAction.Create(Title, c => AddTapir(context.Document, (ClassDeclarationSyntax)node, c), Title), diagnostic);
        }

        private static async Task<Document> AddTapir(Document document, ClassDeclarationSyntax node, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken);

            const string commentTapir = @"
/*              `++-      `-::`                                   
                osso`````/+/::.                                   
              `-ssssooooo+----.............`````                  
            ./osssssssssso+/--------------------....``            
        ``.+sssossssssssssss/:------------------------..``        
       .oossso::-:ssssssssssssso:-------------------------.``     
       `ossss:oy+`+ssssssssssssso----------------------------.    
       /sssss+::.:sssssssssssssss:----------------------------.   
      -ssssssssssssssssssssssssss/------------------------------  
     `ossssssooooooosssssyssyyyys/------------------------:sssss: 
     -sssssoossoooss+ssssyssssssy/----------------------:ssssssss 
     +sssssooo++/:.` /sssysssyyyso---------------------/ssssssso- 
     sssss+.         `-ysyssysssss--------------------/sssssss/   
     sssso``         ` +yyysyyyyyso-------------------sssssss/    
     +sss/             .yyyyssssssss+:-------------:+ssssssso     
     `+ss-             `syyyssssssso-:-----------:osysssssss/     
       ``               oyyysssssso`     `````    `:sysssssso     
                        +yyy/sssss.                .syy/ossss:    
                        oyyo.ssss+                -syy- `-sss-    
                       -yyy+`ssss:              -oyyy:   +so.     
                       /ooo:.ssss.             -oooo:  `/ss.      
                        ````ossss-              ```  `:oss.       
                           `:/::.                    /++/.        */";

            var comment = SyntaxFactory.Comment(commentTapir);

            var result = node.WithLeadingTrivia(
                new[]
                {
                    comment,
                    SyntaxFactory.ElasticCarriageReturnLineFeed,
                }.Concat(node.GetLeadingTrivia()));

            var newRoot = root.ReplaceNode(node, result);

            var newDocument = document.WithSyntaxRoot(newRoot);

            return newDocument;

        }
    }
}