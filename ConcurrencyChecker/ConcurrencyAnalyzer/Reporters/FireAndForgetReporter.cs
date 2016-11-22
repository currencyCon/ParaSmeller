using System.Linq;
using ConcurrencyAnalyzer.Representation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Diagnostic = ConcurrencyAnalyzer.Diagnostics.Diagnostic;

namespace ConcurrencyAnalyzer.Reporters
{
    public class FireAndForgetReporter: BaseReporter
    {
        public const string FireAndForgetCallId = "FaF001";
        private const string ThreadStartDefintion = "System.Threading.Tasks.Task.Run";
        private const string TaskWaitMethodName = "Wait";
        public static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.FireAndForgetAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        public static readonly LocalizableString MessageFormatFireAndForghet = new LocalizableResourceString(nameof(Resources.FireAndForgetAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        public static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.FireAndForgetAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        public const string Category = "Synchronization";

        private void InspectMemberForUnawaitedTasks(Member member)
        {
            foreach (var invocationExpressionRepresentation in member.GetAllInvocations())
            {
                if (invocationExpressionRepresentation.OriginalDefinition == ThreadStartDefintion)
                {
                    CheckForSingleInvocation(invocationExpressionRepresentation);
                    CheckForLostAssignment(invocationExpressionRepresentation, member);
                }
            }
        }

        private void CheckForLostAssignment(InvocationExpressionRepresentation invocationExpressionRepresentation, Member member)
        {
            if (AssignmentIsLost(invocationExpressionRepresentation, member))
            {
                Reports.Add(ReportFireAndForget(invocationExpressionRepresentation.Implementation));
            }
        }

        private static bool AssignmentIsLost(InvocationExpressionRepresentation invocationExpressionRepresentation, Member member)
        {
            return invocationExpressionRepresentation.GetFirstParent<EqualsValueClauseSyntax>() != null &&
                   !AssignmentIsAwaited(invocationExpressionRepresentation, member);
        }

        private static bool AssignmentIsAwaited(InvocationExpressionRepresentation invocationExpressionRepresentation, Member member)
        {
            var assignment = invocationExpressionRepresentation.GetFirstParent<VariableDeclaratorSyntax>();
            if (assignment == null)
            {
                return false;
            }
            var variableName = assignment.Identifier.ToString();
            return TaskIsAwaited(member, variableName);
        }

        private static bool AssignmentIsAwaitedInInvocatedMember(Member member, string variableName)
        {
            var invocationExpressions = member.GetAllInvocations();
            foreach (var invocationExpressionRepresentation in invocationExpressions)
            {
                if (invocationExpressionRepresentation.Type == SymbolKind.Method &&
                    invocationExpressionRepresentation.Arguments.Select(e => e.Identifier.Text)
                        .Contains(variableName))
                {
                    foreach (var calledMethod in invocationExpressionRepresentation.InvokedImplementations.OfType<MethodRepresentation>())
                    {
                        if (calledMethod != null)
                        {
                            return IsAwaitedInMethod(calledMethod);
                        }
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

        private static bool TaskIsAwaited(Member member, string variableName)
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

        private void CheckForSingleInvocation(InvocationExpressionRepresentation invocationExpressionRepresentation)
        {
            if (invocationExpressionRepresentation.GetFirstParent<ExpressionStatementSyntax>() != null)
            {
                Reports.Add(ReportFireAndForget(invocationExpressionRepresentation.Implementation));
            }
        }
        
        private static Diagnostic ReportFireAndForget(SyntaxNode threadInvocation)
        {
            return new Diagnostic(FireAndForgetCallId, Title, MessageFormatFireAndForghet, Description, Category, threadInvocation.GetLocation());

        }

        protected override void Register()
        {
            RegisterMemberReport(InspectMemberForUnawaitedTasks);
        }
    }
}
