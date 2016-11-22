using System.Collections.Generic;
using ConcurrencyAnalyzer.Representation;
using ConcurrencyAnalyzer.SyntaxNodeUtils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Diagnostic = ConcurrencyAnalyzer.Diagnostics.Diagnostic;

namespace ConcurrencyAnalyzer.Reporters
{
    public class ExplicitThreadsReporter: BaseReporter
    {
        public const string DiagnosticId = "ETC001";
        private const string ThreadDefintion = "System.Threading.Thread";
        private readonly ICollection<string> _ignoreDefinitions = new List<string>(new[]
        {
            "System.Threading.Thread.CurrentThread",
            "System.Threading.Thread.CurrentPrincipal",
            "System.Threading.Thread.CurrentContext",
        });
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
            if (symbol?.OriginalDefinition?.ToString() == ThreadDefintion && IsNotCurrentThreadAccess(node, classRepresentation))
            {
                Reports.Add(new Diagnostic(DiagnosticId, Title, MessageFormat, Description, Category, node.Parent.GetLocation()));
            }
        }

        private bool IsNotCurrentThreadAccess(SyntaxNode node, ClassRepresentation classRepresentation)
        {
            var parent = node.Parent;
            do
            {
                var info = classRepresentation.SemanticModel.GetSymbolInfo(parent);
                if (_ignoreDefinitions.Contains(info.Symbol?.OriginalDefinition.ToString()))
                { 
                    return false;
                }

                parent = parent.Parent;
            } while (parent is MemberAccessExpressionSyntax);
           
            return true;
        }

        private void ReportThreadUsage(ClassRepresentation classRepresentation)
        {
            foreach (var identifierName in classRepresentation.Implementation.GetChildren<IdentifierNameSyntax>())
            {
                AnalyzeIdentifier(identifierName, classRepresentation);
            }
        }

        protected override void Register()
        {
            RegisterClassReport(ReportThreadUsage);
        }
    }
}

