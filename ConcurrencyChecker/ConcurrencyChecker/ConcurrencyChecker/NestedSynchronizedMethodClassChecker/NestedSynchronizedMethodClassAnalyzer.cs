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
        public static string NestedLockingDiagnosticId = "NSMC001";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.NSMCAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.NSMCAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.NSMCAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Synchronization";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(NestedLockingDiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationAction(CheckForNestedLocks);
        }

        private static void CheckForNestedLocks(CompilationAnalysisContext context)
        {
            var solutionModel = SolutionRepresentationFactory.Create(context.Compilation);
            var methods = solutionModel.Classes.SelectMany(e => e.SynchronizedMethods);
            foreach (var memberWithBody in methods)
            {
                CheckForLockingOnSameType(context, memberWithBody);
                
            }
            CheckAquireMultipleLocks(context, solutionModel.Classes);
        }

        private static void CheckAquireMultipleLocks(CompilationAnalysisContext context, ICollection<ClassRepresentation> classes)
        {
            
            foreach (var clazz in classes)
            {
                List<List<string>> lockObjects = new List<List<string>>();

                foreach (var memberWithBody in clazz.GetMembersWithMultipleLocks())
                {
                    lockObjects.Add(memberWithBody.GetAllLockPossibilities());
                }

                bool result = CheckAquireList(lockObjects);
                if (result)
                {
                    foreach (var memberWithBody in clazz.GetMembersWithMultipleLocks())
                    {
                        var diagn = Diagnostic.Create(Rule, memberWithBody.Name.GetLocation());
                        context.ReportDiagnostic(diagn);
                    }
                }
            }
        }

        private static bool CheckAquireList(List<List<string>> lockObjects)
        {
            foreach(var l1 in lockObjects)
            {
                foreach (var l2 in lockObjects)
                {
                    if(l1 == l2) continue;

                    if (DifferentLocking(l1, l2))
                        return true;
                }
            }

            return false;
        }

        private static bool DifferentLocking(List<string> l1, List<string> l2)
        {
            if (l1.Count < 2 || l2.Count < 2) return false;

            if (CheckDifferentLockingOnList(l1, l2)) return true;
            if (CheckDifferentLockingOnList(l2, l1)) return true;


            return false;
        }

        private static bool CheckDifferentLockingOnList(List<string> l1, List<string> l2)
        {
            for (int i = 0; i < l1.Count; i++)
            {
                for (int j = i; j < l1.Count; j++)
                {
                    if (i == j) continue;

                    string item1_list1 = l1[i];
                    string item2_list1 = l1[j];

                    int index1_list2 = l2.IndexOf(l2.Find(s => s == item1_list1));
                    int index2_list2 = l2.IndexOf(l2.Find(s => s == item2_list1));

                    if(index1_list2 == -1 || index2_list2 == -1) continue;
                    if (index1_list2 > index2_list2) return true;
                }
            }
            return false;
        }

        private static void CheckForLockingOnSameType(CompilationAnalysisContext context, IMemberWithBody memberWithBody)
        {
            var lockStatements = memberWithBody.GetLockStatements().ToList();
            if (!lockStatements.Any())
            {
                return;
            }
            var method = memberWithBody as MethodRepresentation;
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
                        if (memberAccessExpressionSyntax.Expression.ToString() == parameter.ToString())
                        {
                            if (CheckIfAquiresSameLock(lockObject, memberAccessExpressionSyntax.Name, method.MethodImplementation))
                            {
                                var diagn = Diagnostic.Create(Rule, memberAccessExpressionSyntax.GetLocation());
                                context.ReportDiagnostic(diagn);
                            }
                        }
                    }
                }
            }
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

        private static List<SyntaxToken> ParametersOfOwnType(IMethodRepresentation node, CompilationAnalysisContext context)
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
