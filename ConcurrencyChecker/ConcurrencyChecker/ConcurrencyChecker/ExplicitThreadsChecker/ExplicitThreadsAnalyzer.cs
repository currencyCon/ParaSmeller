using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ConcurrencyChecker.ExplicitThreadsChecker
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ExplicitThreadsAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "ETC001";
        private const string ThreadStartDefintion = "System.Threading.Thread.Start()";
        private const string Category = "ParallelCorrectness";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.ETCAnalyzerTitle), Resources.ResourceManager, typeof (Resources));

        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.ETCAnalyzerMessageFormatSingleLine), Resources.ResourceManager, typeof (Resources));

        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.ETCAnalyzerDescription), Resources.ResourceManager, typeof (Resources));

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true, Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeThreadStart, SyntaxKind.InvocationExpression);
        }

        private static void AnalyzeThreadStart(SyntaxNodeAnalysisContext context)
        {
            var root = context.Node;

            if (!(root is InvocationExpressionSyntax))
            {
                return;
            }
            if (!root.DescendantNodes().OfType<ObjectCreationExpressionSyntax>().Any())
            {
                //Ignore ETC002
                return;
            }
            if (root.DescendantNodes().OfType<ObjectCreationExpressionSyntax>().First().Type.ToString() != "Thread")
            {
                //Ignore ETC002
                return;
            }

            var invocationExpression = root as InvocationExpressionSyntax;

            var methodSymbol = context.SemanticModel.GetSymbolInfo(invocationExpression).Symbol as IMethodSymbol;

            if (methodSymbol == null)
            {
                return;
            }
            if (methodSymbol.OriginalDefinition.ToString() != ThreadStartDefintion)
            {
                return;
            }

            var diagn = Diagnostic.Create(Rule, invocationExpression.GetLocation());
            context.ReportDiagnostic(diagn);
        }
    }
}