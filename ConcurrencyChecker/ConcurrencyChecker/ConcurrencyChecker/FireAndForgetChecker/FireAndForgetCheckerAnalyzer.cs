

using System.Collections.Immutable;
using ConcurrencyAnalyzer.Representation;
using ConcurrencyAnalyzer.RepresentationFactories;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ConcurrencyChecker.FireAndForgetChecker
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class FireAndForgetCheckerAnalyzer: DiagnosticAnalyzer
    {
        public const string FireAndForgetCallId = "FaF001";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.FireAndForgetAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormatFireAndForghet = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormatFireAndForget), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.FireAndForgetAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Synchronization";

        private static readonly DiagnosticDescriptor RuleFireAndForgetCallRule = new DiagnosticDescriptor(FireAndForgetCallId, Title, MessageFormatFireAndForghet, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(RuleFireAndForgetCallRule);
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationAction(CheckForUnawaitedTasks);

        }

        private static void CheckForUnawaitedTasks(CompilationAnalysisContext context)
        {
            var b = context.Options;
            var solutionModel = SolutionRepresentationFactory.Create(context.Compilation);
            foreach (var clazz in solutionModel.Classes)
            {
                InspectClassForUnawaitedTasks(clazz);
            }
        }

        private static void InspectClassForUnawaitedTasks(ClassRepresentation clazz)
        {
            foreach (var memberWithBody in clazz.UnSynchronizedMethods)
            {
                InspectMemberForUnawaitedTasks(memberWithBody);
            }
        }

        private static void InspectMemberForUnawaitedTasks(IMethodRepresentation methodRepresentation)
        {
            var x = methodRepresentation;
            var z = 2;
        }
    }
}
