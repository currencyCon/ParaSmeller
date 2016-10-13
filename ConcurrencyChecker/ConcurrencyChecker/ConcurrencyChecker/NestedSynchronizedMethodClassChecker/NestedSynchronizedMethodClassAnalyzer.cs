using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ConcurrencyAnalyzer.Representation;
using ConcurrencyAnalyzer.RepresentationExtensions;
using ConcurrencyAnalyzer.RepresentationFactories;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;


namespace ConcurrencyChecker.NestedSynchronizedMethodClassChecker
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class NestedSynchronizedMethodCalssAnalyzer : DiagnosticAnalyzer
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
            var methods = solutionModel.Classes.SelectMany(e => e.Members.Where(a => a.Blocks.Any(c => c is LockBlock)));
            foreach (var memberWithBody in methods)
            {
                CheckForLockingOnSameType(context, memberWithBody);
            }
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
                var memberAccessExpression =
                    lockStatementSyntax.DescendantNodes().OfType<MemberAccessExpressionSyntax>();
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
            var clazz = root.AncestorsAndSelf().OfType<ClassDeclarationSyntax>().FirstOrDefault();
            var calledMethod =
                clazz
                    .DescendantNodes()
                    .OfType<MethodDeclarationSyntax>().FirstOrDefault(e => e.Identifier.Text == methodName.ToString());
            var lockStatements = GetLockStatements(calledMethod);
            foreach (var lockStatementSyntax in lockStatements)
            {
                if (lockStatementSyntax.Expression.ToString() == lockObject.ToString())
                {
                    return true;
                }
            }
            return false;
        }

        private static IEnumerable<LockStatementSyntax> GetLockStatements(SyntaxNode node)
        {
            return node.DescendantNodesAndSelf().OfType<LockStatementSyntax>();
        }

        private static List<SyntaxToken> ParametersOfOwnType(MethodRepresentation node, CompilationAnalysisContext context)
        {
            var clazz = GetClass(node.MethodImplementation);
            var model = context.Compilation.GetSemanticModel(node.ContainingClass.ClassDeclarationSyntax.SyntaxTree);
            var classTypeSymbol = model.GetDeclaredSymbol(clazz);
            var parametersOfOwnType = new List<SyntaxToken>();
            HierarchieChecker hierarchieChecker = new HierarchieChecker(classTypeSymbol);

            foreach (var parameterSyntax in node.MethodImplementation.ParameterList.Parameters)
            {
                var baseTypeSymbol = model.GetDeclaredSymbol(parameterSyntax).Type;
                if (hierarchieChecker.IsSubClass(baseTypeSymbol))
                {
                    parametersOfOwnType.Add(parameterSyntax.Identifier);
                }
            }
            return parametersOfOwnType;
        }


        private static ClassDeclarationSyntax GetClass(SyntaxNode method)
        {
            var classDeclarations = method.AncestorsAndSelf().OfType<ClassDeclarationSyntax>();
            var classDeclaration = classDeclarations.FirstOrDefault();
            return classDeclaration;
        }
    }
}
