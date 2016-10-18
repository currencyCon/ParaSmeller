

using System.Collections.Immutable;
using ConcurrencyAnalyzer.Representation;
using ConcurrencyAnalyzer.RepresentationFactories;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ConcurrencyChecker.FireAndForgetChecker
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class FireAndForgetCheckerAnalyzer: DiagnosticAnalyzer
    {
        public const string FireAndForgetCallId = "FaF001";
        private const string ThreadStartDefintion = "System.Threading.Tasks.Task.Run";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.FireAndForgetAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormatFireAndForghet = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormatFireAndForget), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.FireAndForgetAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Synchronization";

        private static readonly DiagnosticDescriptor RuleFireAndForgetCall = new DiagnosticDescriptor(FireAndForgetCallId, Title, MessageFormatFireAndForghet, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(RuleFireAndForgetCall);
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationAction(CheckForUnawaitedTasks);

        }

        private static void CheckForUnawaitedTasks(CompilationAnalysisContext context)
        {
            var solutionModel = SolutionRepresentationFactory.Create(context.Compilation);
            foreach (var clazz in solutionModel.Classes)
            {
                InspectClassForUnawaitedTasks(clazz, context);
            }
        }

        private static void InspectClassForUnawaitedTasks(ClassRepresentation clazz, CompilationAnalysisContext context)
        {
            foreach (var memberWithBody in clazz.Members)
            {
                InspectMemberForUnawaitedTasks(memberWithBody, context);
            }
        }

        private static void InspectMemberForUnawaitedTasks(IMemberWithBody methodRepresentation, CompilationAnalysisContext context)
        {
            foreach (var invocationExpressionRepresentation in methodRepresentation.InvocationExpressions)
            {
                if (invocationExpressionRepresentation.OriginalDefinition == ThreadStartDefintion)
                {
                    if (!(invocationExpressionRepresentation.Implementation.Parent is EqualsValueClauseSyntax))
                    {
                        ReportFireAndForget(context, invocationExpressionRepresentation.Implementation);
                    }
                }
            }
        }

        private static void ReportFireAndForget(CompilationAnalysisContext context,
CSharpSyntaxNode threadInvocation)
        {

            var diagnostic = Diagnostic.Create(RuleFireAndForgetCall, threadInvocation.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }
}
