using System.Collections.Immutable;
using System.Linq;
using ConcurrencyChecker;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ExplicitThreadsChecker
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ExplicitThreadsMultilineCheckerAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "ETC002";
        private const string ThreadStartDefintion = "System.Threading.Thread.Start()";
        private const string Category = "ParallelCorrectness";

        private static readonly LocalizableString Title = new LocalizableResourceString(
            nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof (Resources));

        private static readonly LocalizableString MessageFormat =
            new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager,
                typeof (Resources));

        private static readonly LocalizableString Description =
            new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager,
                typeof (Resources));

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category, DiagnosticSeverity.Warning, true, Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeThreadStart, SyntaxKind.SimpleMemberAccessExpression);
        }

        private static void AnalyzeThreadStart(SyntaxNodeAnalysisContext context)
        {
            var root = context.Node;

            if (!(root is MemberAccessExpressionSyntax))
            {
                return;
            }
            if (root.DescendantNodes().OfType<ObjectCreationExpressionSyntax>().Any())
            {
                if (root.DescendantNodes().OfType<ObjectCreationExpressionSyntax>().First().Type.ToString() == "Thread")
                {
                    //Ignore ETC001
                    return;
                }
            }

            var callingMethod = root as MemberAccessExpressionSyntax;

            var methodSymbol = context.SemanticModel.GetSymbolInfo(root).Symbol as IMethodSymbol;

            if (methodSymbol == null)
            {
                return;
            }
            if (methodSymbol.OriginalDefinition.ToString() != ThreadStartDefintion)
            {
                return;
            }

            CheckThreadUsage(context, root, callingMethod.Expression);
        }


        private static void CheckThreadUsage(SyntaxNodeAnalysisContext context, SyntaxNode root,
            ExpressionSyntax identifier)
        {
            var block = root.Ancestors().OfType<BlockSyntax>().First();
            var references =
                block.DescendantNodesAndSelf()
                    .OfType<IdentifierNameSyntax>()
                    .Where(i => i.Identifier.ToString() == identifier.ToString());

            if (!IsThreadDeclaredInBlock(block, identifier))
            {
                return;
            }

            var logWarning = true;
            foreach (var identifierNameSyntax in references)
            {
                if (identifierNameSyntax.Parent is MemberAccessExpressionSyntax)
                {
                    var method = (MemberAccessExpressionSyntax) identifierNameSyntax.Parent;
                    var methodName = method.Name.ToString();
                    if (methodName != "Start")
                    {
                        logWarning = false;
                    }
                }
                else if (!(identifierNameSyntax.Parent is LocalDeclarationStatementSyntax) &&
                         !(identifierNameSyntax.Parent is AssignmentExpressionSyntax))
                {
                    logWarning = false;
                }
            }


            if (logWarning)
            {
                var diagn = Diagnostic.Create(Rule, root.GetLocation(), identifier.ToString());
                context.ReportDiagnostic(diagn);
            }
        }

        private static bool IsThreadDeclaredInBlock(BlockSyntax block, ExpressionSyntax identifier)
        {
            return
                block
                    .DescendantNodes()
                    .OfType<VariableDeclaratorSyntax>()
                    .Any(v => v.Identifier.ToString() == identifier.ToString());
        }
    }
}