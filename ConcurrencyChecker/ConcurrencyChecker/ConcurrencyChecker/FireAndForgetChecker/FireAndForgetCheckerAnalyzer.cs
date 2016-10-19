using System.Collections.Immutable;
using System.Linq;
using ConcurrencyAnalyzer.Representation;
using ConcurrencyAnalyzer.RepresentationExtensions;
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
        private const string TaskWaitMethodName = "Wait";
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

        private static void InspectMemberForUnawaitedTasks(IMemberWithBody member, CompilationAnalysisContext context)
        {
            foreach (var invocationExpressionRepresentation in member.InvocationExpressions)
            {
                if (invocationExpressionRepresentation.OriginalDefinition == ThreadStartDefintion)
                {
                    CheckForSingleInvocation(context, invocationExpressionRepresentation);
                    CheckForLostAssignment(context, invocationExpressionRepresentation, member);
                }
            }
        }

        private static void CheckForLostAssignment(CompilationAnalysisContext context, IInvocationExpressionRepresentation invocationExpressionRepresentation, IMemberWithBody member)
        {
            if (invocationExpressionRepresentation.GetFirstParent<EqualsValueClauseSyntax>() != null)
            {
                if (!AssignmentIsAwaited(invocationExpressionRepresentation, member))
                {
                    ReportFireAndForget(context, invocationExpressionRepresentation.Implementation);
                }
            }
        }

        private static bool AssignmentIsAwaited(IInvocationExpressionRepresentation invocationExpressionRepresentation, IMemberWithBody member)
        {
            var assignment = invocationExpressionRepresentation.GetFirstParent<VariableDeclaratorSyntax>();
            if (assignment == null)
            {
                return false;
            }
            var variableName = assignment.Identifier.ToString();
            return TaskIsAwaited(member, variableName);
        }

        private static bool AssignmentIsAwaitedInInvocatedMember(IMemberWithBody member, string variableName)
        {
            var invocationExpressions = member.Blocks.FirstOrDefault().InvocationExpressions;
            foreach (var invocationExpressionRepresentation in invocationExpressions)
            {
                if (invocationExpressionRepresentation.Type == SymbolKind.Method &&
                    invocationExpressionRepresentation.Arguments.Select(e => e.Identifier.Text)
                        .Contains(variableName))
                {
                    var calledMethod =
                        invocationExpressionRepresentation.InvocationImplementation as IMethodRepresentation;
                    if (calledMethod != null)
                    {
                        var paramsOfCorrectType = calledMethod.Parameters.Where(e => e.Type.ToString() == "Task");
                        var taskIsWaited = false;
                        foreach (var parameterSyntax in paramsOfCorrectType)
                        {
                            if (TaskIsAwaited(calledMethod, parameterSyntax.Identifier.Text))
                            {
                                taskIsWaited = true;
                            }
                        }
                        return taskIsWaited;
                    }
                }
            }
            return false;
        }

        private static bool TaskIsAwaited(IMemberWithBody member, string variableName)
        {
            var simpleMemberAccesses = member.GetChildren<MemberAccessExpressionSyntax>();
            foreach (var memberAccessExpressionSyntax in simpleMemberAccesses)
            {
                var operationName = memberAccessExpressionSyntax.Name.ToString();
                var variable = memberAccessExpressionSyntax.Expression.ToString();
                if (operationName == TaskWaitMethodName && variable == variableName)
                {
                    return true;
                }
            }
            return AssignmentIsAwaitedInInvocatedMember(member, variableName);
        }

        private static void CheckForSingleInvocation(CompilationAnalysisContext context, IInvocationExpressionRepresentation invocationExpressionRepresentation)
        {
            if (invocationExpressionRepresentation.GetFirstParent<ExpressionStatementSyntax>() != null)
            {
                ReportFireAndForget(context, invocationExpressionRepresentation.Implementation);
            }
        }

        private static void ReportFireAndForget(CompilationAnalysisContext context, CSharpSyntaxNode threadInvocation)
        {

            var diagnostic = Diagnostic.Create(RuleFireAndForgetCall, threadInvocation.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }
}
