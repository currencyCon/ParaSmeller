using System.Linq;
using ConcurrencyAnalyzer.Representation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Diagnostic = ConcurrencyAnalyzer.Diagnostics.Diagnostic;

namespace ConcurrencyAnalyzer.Reporters
{
    public class ExplicitThreadsReporter: BaseReporter
    {
        public const string DiagnosticId = "ETC001";
        private const string ThreadDefintion = "System.Threading.Thread";
        public const string Category = "ParallelCorrectness";

        public static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.ETCAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        public static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.ETCAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        public static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.ETCAnalyzerDescription), Resources.ResourceManager, typeof(Resources));

        private void AnalyzeIdentifier(SyntaxNode node, ClassRepresentation classRepresentation)
        {
            INamedTypeSymbol symbol = null;

            var info = classRepresentation.SemanticModel.GetSymbolInfo(node);
            if (info.Symbol != null)
            {
                symbol = info.Symbol as INamedTypeSymbol;
            }
            if (symbol?.OriginalDefinition?.ToString() == ThreadDefintion)
            {
                Reports.Add(new Diagnostic(DiagnosticId, Title, MessageFormat, Description, Category,
                    node.Parent.GetLocation()));
            }
        }
        private void ReportThreadUsage(ClassRepresentation classRepresentation)
        {
            foreach (var identifierName in classRepresentation.Implementation.DescendantNodes().OfType<IdentifierNameSyntax>())
            {
                AnalyzeIdentifier(identifierName, classRepresentation);
            }
        }

        public override void Register()
        {
            RegisterClassReport(ReportThreadUsage);
        }
    }
}
