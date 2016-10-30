using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ConcurrencyAnalyzer.Representation;
using ConcurrencyAnalyzer.RepresentationExtensions;
using ConcurrencyAnalyzer.RepresentationFactories;
using ConcurrencyAnalyzer.SyntaxFilters;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;


namespace ConcurrencyChecker.NestedSynchronizedMethodClassChecker
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class NestedSynchronizedMethodClassAnalyzer : DiagnosticAnalyzer
    {
        public const string NestedLockingDiagnosticId = "NSMC001";
        public const string NestedLockingDiagnosticId2 = "NSMC002";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.NSMCAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.NSMCAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.NSMCAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Synchronization";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(NestedLockingDiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);
        private static readonly DiagnosticDescriptor Rule2 = new DiagnosticDescriptor(NestedLockingDiagnosticId2, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule, Rule2);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationAction(CheckForNestedLocks);
        }

        private static async void CheckForNestedLocks(CompilationAnalysisContext context)
        {
            var solutionModel = await SolutionRepresentationFactory.Create(context.Compilation);
            var methods = solutionModel.Classes.SelectMany(e => e.SynchronizedMethods);
            foreach (var memberWithBody in methods)
            {
                CheckForLockingOnSameType(context, memberWithBody);
                
            }
            CheckAquireMultipleLocks(context, solutionModel.Classes);
        }

        private static void CheckAquireMultipleLocks(CompilationAnalysisContext context, IEnumerable<ClassRepresentation> classes)
        {
            
            foreach (var clazz in classes)
            {
                var lockObjects = new List<List<string>>();

                foreach (var memberWithBody in clazz.GetMembersWithMultipleLocks())
                {
                    lockObjects.Add(memberWithBody.GetAllLockPossibilities());
                }

                bool correct = LockChecker.IsCorrectAquired(lockObjects);
                if (!correct)
                {
                    foreach (var memberWithBody in clazz.GetMembersWithMultipleLocks())
                    {
                        var diagn = Diagnostic.Create(Rule2, memberWithBody.Name.GetLocation());
                        context.ReportDiagnostic(diagn);
                    }
                }
            }
        }
        
        private static void CheckForLockingOnSameType(CompilationAnalysisContext context, IMember member)
        {
            var lockStatements = member.GetLockStatements().ToList();
            if (!lockStatements.Any())
            {
                return;
            }
            var method = member as MethodRepresentation;
            if (method == null)
            {
                return;
            }
            var parametersOfOwnKind = ParametersOfOwnType(method, context);
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
                            method.MethodImplementation))
                        {
                            var diagn = Diagnostic.Create(Rule, memberAccessExpressionSyntax.GetLocation());
                            context.ReportDiagnostic(diagn);
                        }
                    }
                }
            }
        }

        private static bool UsesLockRecursive(MemberAccessExpressionSyntax memberAccessExpressionSyntax,
            SyntaxToken parameter, ExpressionSyntax lockObject,
            SyntaxNode methodDeclarationSyntax)
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

        private static List<SyntaxToken> ParametersOfOwnType(MethodRepresentation node, CompilationAnalysisContext context)
        {
            var clazz = node.GetFirstParent<ClassDeclarationSyntax>();
            var model = context.Compilation.GetSemanticModel(node.ContainingClass.ClassDeclarationSyntax.SyntaxTree);
            var classTypeSymbol = model.GetDeclaredSymbol(clazz);
            var parametersOfOwnType = new List<SyntaxToken>();
            var hierarchieChecker = new HierarchieChecker(classTypeSymbol);

            foreach (var parameterSyntax in node.Parameters)
            {
                var baseTypeSymbol = model.GetDeclaredSymbol(parameterSyntax).Type;
                if (hierarchieChecker.IsSubClass(baseTypeSymbol))
                {
                    parametersOfOwnType.Add(parameterSyntax.Identifier);
                }
            }
            return parametersOfOwnType;
        }
    }
}
