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
        private const string ThreadDefintion = "System.Threading.Thread";
        private const string ThreadStartDefintion = "System.Threading.Thread.Start()";
        private const string Category = "ParallelCorrectness";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.ETCAnalyzerTitle), Resources.ResourceManager, typeof (Resources));
        public static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.ETCAnalyzerMessageFormat), Resources.ResourceManager, typeof (Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.ETCAnalyzerDescription), Resources.ResourceManager, typeof (Resources));
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true, Description);
        
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationAction(CheckForThreads);
        }

        private static async void CheckForThreads(CompilationAnalysisContext context)
        {
            var solutionModel = await SolutionRepresentationFactory.Create(context.Compilation);
            foreach (var classRepresentation in solutionModel.Classes)
            {
                foreach (var identifierName in classRepresentation.Implementation.DescendantNodes().OfType<IdentifierNameSyntax>())
                {
                    AnalyzeIdentifier(identifierName, context);
                }
            }
        }

        private static void AnalyzeIdentifier(IdentifierNameSyntax node, CompilationAnalysisContext context)
        {
            var root = node.GetNamedSymbol(context);
            if (root?.OriginalDefinition?.ToString() == ThreadDefintion)
            {
                var diagn = Diagnostic.Create(Rule, node.Parent.GetLocation());
                context.ReportDiagnostic(diagn);
            }
        }
        
    }
}