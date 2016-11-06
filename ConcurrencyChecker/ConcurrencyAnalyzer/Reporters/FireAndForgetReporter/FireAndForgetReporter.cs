
using System.Collections.Generic;
using System.Linq;
using ConcurrencyAnalyzer.Representation;
using ConcurrencyAnalyzer.RepresentationExtensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Diagnostic = ConcurrencyAnalyzer.Diagnostics.Diagnostic;

namespace ConcurrencyAnalyzer.Reporters.FireAndForgetReporter
{
    public class FireAndForgetReporter: IReporter
    {
        public const string FireAndForgetCallId = "FaF001";
        private const string ThreadStartDefintion = "System.Threading.Tasks.Task.Run";
        private const string TaskWaitMethodName = "Wait";
        public static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.FireAndForgetAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        public static readonly LocalizableString MessageFormatFireAndForghet = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormatFireAndForget), Resources.ResourceManager, typeof(Resources));
        public static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.FireAndForgetAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        public const string Category = "Synchronization";

        private static void InspectClassForUnawaitedTasks(ClassRepresentation clazz, List<Diagnostic> reports)
        {
            foreach (var memberWithBody in clazz.Members)
            {
                InspectMemberForUnawaitedTasks(memberWithBody, reports);
            }
        }

        private static void InspectMemberForUnawaitedTasks(IMember member, List<Diagnostic> reports)
        {
            foreach (var invocationExpressionRepresentation in member.InvocationExpressions)
            {
                if (invocationExpressionRepresentation.OriginalDefinition == ThreadStartDefintion)
                {
                    CheckForSingleInvocation(invocationExpressionRepresentation, reports);
                    CheckForLostAssignment(invocationExpressionRepresentation, member, reports);
                }
            }
        }

        private static void CheckForLostAssignment(InvocationExpressionRepresentation invocationExpressionRepresentation, IMember member, List<Diagnostic> reports)
        {
            if (AssignmentIsLost(invocationExpressionRepresentation, member))
            {
                reports.Add(ReportFireAndForget(invocationExpressionRepresentation.Implementation));
            }
        }

        private static bool AssignmentIsLost(InvocationExpressionRepresentation invocationExpressionRepresentation, IMember member)
        {
            return invocationExpressionRepresentation.GetFirstParent<EqualsValueClauseSyntax>() != null &&
                   !AssignmentIsAwaited(invocationExpressionRepresentation, member);
        }

        private static bool AssignmentIsAwaited(InvocationExpressionRepresentation invocationExpressionRepresentation, IMember member)
        {
            var assignment = invocationExpressionRepresentation.GetFirstParent<VariableDeclaratorSyntax>();
            if (assignment == null)
            {
                return false;
            }
            var variableName = assignment.Identifier.ToString();
            return TaskIsAwaited(member, variableName);
        }

        private static bool AssignmentIsAwaitedInInvocatedMember(IMember member, string variableName)
        {
            var invocationExpressions = member.Blocks.FirstOrDefault().InvocationExpressions;
            foreach (var invocationExpressionRepresentation in invocationExpressions)
            {
                if (invocationExpressionRepresentation.Type == SymbolKind.Method &&
                    invocationExpressionRepresentation.Arguments.Select(e => e.Identifier.Text)
                        .Contains(variableName))
                {
                    var calledMethod =
                        invocationExpressionRepresentation.InvokedImplementation as MethodRepresentation;
                    if (calledMethod != null)
                    {
                        return IsAwaitedInMethod(calledMethod);
                    }
                }
            }
            return false;
        }

        private static bool IsAwaitedInMethod(MethodRepresentation calledMethod)
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

        private static bool TaskIsAwaited(IMember member, string variableName)
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

        private static void CheckForSingleInvocation(InvocationExpressionRepresentation invocationExpressionRepresentation, List<Diagnostic> reports)
        {
            if (invocationExpressionRepresentation.GetFirstParent<ExpressionStatementSyntax>() != null)
            {
                reports.Add(ReportFireAndForget(invocationExpressionRepresentation.Implementation));
            }
        }


        private static Diagnostic ReportFireAndForget(SyntaxNode threadInvocation)
        {
            return new Diagnostic(FireAndForgetCallId, Title, MessageFormatFireAndForghet, Description, Category, threadInvocation.GetLocation());

        }
        public ICollection<Diagnostic> Report(SolutionRepresentation solutionRepresentation)
        {
            var reports = new List<Diagnostic>();
            foreach (var clazz in solutionRepresentation.Classes)
            {
                InspectClassForUnawaitedTasks(clazz, reports);
            }
            return reports;
        }
    }
}
