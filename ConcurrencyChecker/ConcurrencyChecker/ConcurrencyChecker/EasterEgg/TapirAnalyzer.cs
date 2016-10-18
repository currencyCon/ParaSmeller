using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ConcurrencyChecker.EasterEgg
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TapirrAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "TAPIR001";
        private const string Category = "ParallelCorrectness";
        private const string TapirClass = "Tapir";

        private static readonly LocalizableString Title = new LocalizableResourceString(
            nameof(Resources.TapirAnalyzerTitle), Resources.ResourceManager, typeof (Resources));

        private static readonly LocalizableString MessageFormat =
            new LocalizableResourceString(nameof(Resources.TapirAnalyzerMessageFormat), Resources.ResourceManager,
                typeof (Resources));

        private static readonly LocalizableString Description =
            new LocalizableResourceString(nameof(Resources.TapirAnalyzerDescription), Resources.ResourceManager,
                typeof (Resources));

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category, DiagnosticSeverity.Warning, true, Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeClass, SyntaxKind.ClassDeclaration);
        }

        private static void AnalyzeClass(SyntaxNodeAnalysisContext context)
        {
            var root = context.Node;

            if (!(root is ClassDeclarationSyntax))
            {
                return;
            }
            var node = (ClassDeclarationSyntax) root;
            if (node.Identifier.ToString() != TapirClass)
            {
                return;
            }

            var diagn = Diagnostic.Create(Rule, node.Identifier.GetLocation());
            context.ReportDiagnostic(diagn);
        }
    }
}