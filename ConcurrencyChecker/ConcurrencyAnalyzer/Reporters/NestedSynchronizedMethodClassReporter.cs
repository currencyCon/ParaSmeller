using System.Collections.Generic;
using System.Linq;
using ConcurrencyAnalyzer.Locks;
using ConcurrencyAnalyzer.Representation;
using ConcurrencyAnalyzer.RepresentationExtensions;
using ConcurrencyAnalyzer.SymbolExtensions;
using ConcurrencyAnalyzer.SyntaxNodeUtils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Diagnostic = ConcurrencyAnalyzer.Diagnostics.Diagnostic;

namespace ConcurrencyAnalyzer.Reporters
{
    public class NestedSynchronizedMethodClassReporter: BaseReporter
    {
        public const string NestedLockingDiagnosticId = "NSMC001";
        public const string NestedLockingDiagnosticId2 = "NSMC002";

        public static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.NSMCAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        public static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.NSMCAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        public static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.NSMCAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        public const string Category = "Synchronization";

        private void CheckAquireMultipleLocks(ClassRepresentation clazz)
        {
            var lockObjects = new List<List<string>>();

            foreach (var memberWithBody in clazz.GetMembersWithMultipleLocks())
            {
                lockObjects.Add(memberWithBody.GetAllLockArguments());
            }

            bool correct = LockChecker.IsCorrectAquired(lockObjects);
            if (!correct)
            {
                foreach (var memberWithBody in clazz.GetMembersWithMultipleLocks())
                {
                    Reports.Add(new Diagnostic(NestedLockingDiagnosticId2, Title, MessageFormat, Description, Category, memberWithBody.Name.GetLocation()));
                }
            }
        }

        private void CheckForLockingOnSameType(MethodRepresentation method)
        {
            var lockStatements = method.GetLockStatements().ToList();
            if (!lockStatements.Any())
            {
                return;
            }
            var parametersOfOwnKind = ParametersOfOwnType(method);
            if (!parametersOfOwnKind.Any())
            {
                return;
            }
            foreach (var lockStatementSyntax in lockStatements)
            {
                var lockObject = lockStatementSyntax.Expression;
                var memberAccessExpression = lockStatementSyntax.GetChildren<MemberAccessExpressionSyntax>();
                foreach (var memberAccessExpressionSyntax in memberAccessExpression)
                {
                    foreach (var parameter in parametersOfOwnKind)
                    {
                        if (UsesLockRecursive(memberAccessExpressionSyntax, parameter, lockObject,
                            method.Implementation))
                        {
                            Reports.Add(new Diagnostic(NestedLockingDiagnosticId, Title, MessageFormat, Description, Category, memberAccessExpressionSyntax.GetLocation()));
                        }
                    }
                }
            }
        }

        private static bool UsesLockRecursive(MemberAccessExpressionSyntax memberAccessExpressionSyntax, SyntaxToken parameter, ExpressionSyntax lockObject, SyntaxNode methodDeclarationSyntax)
        {
            return memberAccessExpressionSyntax.Expression.ToString() == parameter.ToString() &&
                   CheckIfAquiresSameLock(lockObject, memberAccessExpressionSyntax.Name, methodDeclarationSyntax);
        }
        private static bool CheckIfAquiresSameLock(ExpressionSyntax lockObject, SimpleNameSyntax methodName, SyntaxNode root)
        {
            var clazz = root.GetFirstParent<ClassDeclarationSyntax>();
            var calledMethod =
                clazz.GetChildren<MethodDeclarationSyntax>().FirstOrDefault(e => e.Identifier.Text == methodName.ToString());
            var lockStatements = calledMethod.GetChildren<LockStatementSyntax>();
            foreach (var lockStatementSyntax in lockStatements)
            {
                if (lockStatementSyntax.Expression.ToString() == lockObject.ToString())
                {
                    return true;
                }
            }
            return false;
        }

        private static List<SyntaxToken> ParametersOfOwnType(MethodRepresentation method)
        {
            var semanticModel = method.ContainingClass.SemanticModel;
            var clazz = method.ContainingClass.Implementation;
            var classTypeSymbol = semanticModel.GetDeclaredSymbol(clazz) as INamedTypeSymbol;
            var parametersOfOwnType = new List<SyntaxToken>();
            var hierarchieChecker = new HierarchieChecker(classTypeSymbol);

            foreach (var parameterSyntax in method.Parameters)
            {
                var baseTypeSymbol = semanticModel.GetDeclaredSymbol(parameterSyntax) as IParameterSymbol;
                if (baseTypeSymbol != null && hierarchieChecker.IsSubClass(baseTypeSymbol.Type))
                {
                    parametersOfOwnType.Add(parameterSyntax.Identifier);
                }
            }
            return parametersOfOwnType;
        }

        protected override void Register()
        {
            RegisterClassReport(CheckAquireMultipleLocks);
            RegisterClassReport(CheckForLockingOnSameType);
        }

        private void CheckForLockingOnSameType(ClassRepresentation classRepresentation)
        {
            foreach (var synchronizedMethod in classRepresentation.SynchronizedMethods)
            {
                CheckForLockingOnSameType(synchronizedMethod);
            }
        }
    }
}
