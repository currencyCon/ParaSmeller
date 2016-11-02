using System.Collections.Immutable;
using System.Linq;
using ConcurrencyAnalyzer.RepresentationFactories;
using ConcurrencyAnalyzer.SyntaxNodeUtils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ConcurrencyChecker.ExplicitThreadsChecker
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ExplicitThreadsAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "ETC001";
        public const string DiagnosticIdMultiline = "ETC002";
        private const string ThreadStartDefintion = "System.Threading.Thread.Start()";
        private const string Category = "ParallelCorrectness";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.ETCAnalyzerTitle), Resources.ResourceManager, typeof (Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.ETCAnalyzerMessageFormatSingleLine), Resources.ResourceManager, typeof (Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.ETCAnalyzerDescription), Resources.ResourceManager, typeof (Resources));
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true, Description);

        private static readonly LocalizableString MessageFormatMultiline = new LocalizableResourceString(nameof(Resources.ETCAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly DiagnosticDescriptor RuleMultiline = new DiagnosticDescriptor(DiagnosticIdMultiline, Title, MessageFormatMultiline, Category, DiagnosticSeverity.Warning, true, Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule, RuleMultiline);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationAction(CheckForThreads);
        }

        private static async void CheckForThreads(CompilationAnalysisContext context)
        {
            var solutionModel = await SolutionRepresentationFactory.Create(context.Compilation);
            foreach (var classRepresentation in solutionModel.Classes)
            {
                foreach (var invocationSyntax in classRepresentation.Implementation.DescendantNodes().OfType<MemberAccessExpressionSyntax>())
                {
                    AnalyzeThreadStart(invocationSyntax, context);
                }
            }
        }

        private static void AnalyzeThreadStart(MemberAccessExpressionSyntax root, CompilationAnalysisContext context)
        {
            if (root.DescendantNodes().OfType<ObjectCreationExpressionSyntax>().Any(e => e.Type.ToString() == "Thread"))
            {
                AnalyzeSingleLineThreadStart(root, context);
            }
            else
            {
                AnalyzeMultilineThreadStart(root, context);
            }
        }

        private static void AnalyzeMultilineThreadStart(MemberAccessExpressionSyntax root, CompilationAnalysisContext context)
        {
            var methodSymbol = root.GetMethodSymbol(context);

            if (methodSymbol != null && methodSymbol.OriginalDefinition.ToString() == ThreadStartDefintion)
            {
                CheckThreadUsage(context, root, root.Expression);
            }
        }

        private static void AnalyzeSingleLineThreadStart(MemberAccessExpressionSyntax root, CompilationAnalysisContext context)
        {
            var parent = root.Parent;
            var methodSymbol2 = parent.GetMethodSymbol(context);

            if (methodSymbol2 != null && methodSymbol2.OriginalDefinition.ToString() == ThreadStartDefintion)
            {
                var diagn = Diagnostic.Create(Rule, parent.GetLocation());
                context.ReportDiagnostic(diagn);
            }
        }


        private static void CheckThreadUsage(CompilationAnalysisContext context, SyntaxNode root, ExpressionSyntax identifier)
        {
            var block = root.Ancestors().OfType<BlockSyntax>().First();
            var references = block.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>().Where(i => i.Identifier.ToString() == identifier.ToString());

            if (!IsThreadDeclaredInBlock(block, identifier))
            {
                return;
            }

            var logWarning = true;
            foreach (var identifierNameSyntax in references)
            {
                if (identifierNameSyntax.Parent is MemberAccessExpressionSyntax)
                {
                    var method = (MemberAccessExpressionSyntax)identifierNameSyntax.Parent;
                    var methodName = method.Name.ToString();
                    if (methodName != "Start")
                    {
                        logWarning = false;
                    }
                }
                else if (!(identifierNameSyntax.Parent is LocalDeclarationStatementSyntax) && !(identifierNameSyntax.Parent is AssignmentExpressionSyntax))
                {
                    logWarning = false;
                }
            }

            if (logWarning)
            {
                var diagn = Diagnostic.Create(RuleMultiline, root.GetLocation(), identifier.ToString());
                context.ReportDiagnostic(diagn);
            }
        }

        private static bool IsThreadDeclaredInBlock(BlockSyntax block, ExpressionSyntax identifier)
        {
            return block.DescendantNodes().OfType<VariableDeclaratorSyntax>().Any(v => v.Identifier.ToString() == identifier.ToString());
        }
    }
}