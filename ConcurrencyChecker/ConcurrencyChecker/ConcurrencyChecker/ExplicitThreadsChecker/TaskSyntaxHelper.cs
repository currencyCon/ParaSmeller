using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ExplicitThreadsChecker
{
    internal class TaskSyntaxHelper
    {
        public static InvocationExpressionSyntax CreateInvocationExpressionSyntax(
            ParenthesizedLambdaExpressionSyntax lambda)
        {
            var taskRunSyntax = CreateTaskRun();

            var argument = SyntaxFactory.Argument(lambda);
            var argumentList = SyntaxFactory.SeparatedList(new[] {argument});
            var invocationStatement = SyntaxFactory.InvocationExpression(taskRunSyntax,
                SyntaxFactory.ArgumentList(argumentList));
            return invocationStatement;
        }

        public static InvocationExpressionSyntax CreateInvocationExpressionSyntax(IdentifierNameSyntax methodName)
        {
            var taskRunSyntax = CreateTaskRun();
            var emptyParameterList = SyntaxFactory.ParameterList();

            var compute = SyntaxFactory.IdentifierName(methodName.Identifier.ToString());
            var lambdaBody = SyntaxFactory.InvocationExpression(compute);

            var parenthesizedLambdaExpression = SyntaxFactory.ParenthesizedLambdaExpression(emptyParameterList,
                lambdaBody);
            var argument = SyntaxFactory.Argument(parenthesizedLambdaExpression);
            var argumentList = SyntaxFactory.SeparatedList(new[] {argument});
            var invocationStatement = SyntaxFactory.InvocationExpression(taskRunSyntax,
                SyntaxFactory.ArgumentList(argumentList));
            return invocationStatement;
        }

        public static MemberAccessExpressionSyntax CreateTaskRun()
        {
            var task = SyntaxFactory.IdentifierName("Task");
            var run = SyntaxFactory.IdentifierName("Run");
            var taskRunSyntax = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, task, run);
            return taskRunSyntax;
        }

        public static InvocationExpressionSyntax CreateInvocationStatement(ArgumentSyntax threadArgument)
        {
            InvocationExpressionSyntax invocationStatement;
            if (threadArgument.ChildNodes().OfType<IdentifierNameSyntax>().Any())
            {
                var methodName = threadArgument.ChildNodes().OfType<IdentifierNameSyntax>().First();
                invocationStatement = CreateInvocationExpressionSyntax(methodName);
            }
            else
            {
                var lambda = threadArgument.ChildNodes().OfType<ParenthesizedLambdaExpressionSyntax>().First();
                invocationStatement = CreateInvocationExpressionSyntax(lambda);
            }
            return invocationStatement;
        }
    }
}