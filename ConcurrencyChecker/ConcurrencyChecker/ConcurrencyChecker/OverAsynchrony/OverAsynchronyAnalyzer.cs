using System.Collections.Immutable;
using System.Linq;
using ConcurrencyAnalyzer.Representation;
using ConcurrencyAnalyzer.RepresentationExtensions;
using ConcurrencyAnalyzer.RepresentationFactories;
using ConcurrencyAnalyzer.SyntaxFilters;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ConcurrencyChecker.OverAsynchrony
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class OverAsynchronyAnalyzer : DiagnosticAnalyzer
    {
        private const string Category = "Synchronization";
        public static string DiagnosticId = "OA001";
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.OAAnalyzerTitle), Resources.ResourceManager, typeof (Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.OAAnalyzerMessageFormat), Resources.ResourceManager, typeof (Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.OAAnalyzerDescription), Resources.ResourceManager, typeof (Resources));
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true, Description);
       
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationAction(CheckForPrivateAsyncs);
        }

        private static async void CheckForPrivateAsyncs(CompilationAnalysisContext context)
        {
            var solutionModel = await SolutionRepresentationFactory.Create(context.Compilation);

            foreach (var clazz in solutionModel.Classes)
            {
                foreach (var method in clazz.Methods)
                {
                    CheckForPrivateAsync(method, context);   
                }
            }
        }

        private static void CheckForPrivateAsync(MethodRepresentation method, CompilationAnalysisContext context)
        {
            var symbol = (IMethodSymbol)context.Compilation.GetSemanticModel(method.MethodImplementation.SyntaxTree).GetDeclaredSymbol(method.MethodImplementation);

            if(symbol.IsAsync && symbol.DeclaredAccessibility != Accessibility.Public)
            {
                var diagnostic = Diagnostic.Create(Rule, method.MethodImplementation.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }
        
    }
}