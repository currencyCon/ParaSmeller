using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ConcurrencyAnalyzer.Representation;
using ConcurrencyAnalyzer.RepresentationExtensions;
using ConcurrencyAnalyzer.RepresentationFactories;
using ConcurrencyAnalyzer.SyntaxFilters;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ConcurrencyChecker.OverAsynchrony
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class OverAsynchronyAnalyzer : DiagnosticAnalyzer
    {
        private const string Category = "Synchronization";
        public static string DiagnosticId = "OA001";
        public static string DiagnosticIdNestedAsync = "OA002";
        public static int MAX_DEPTH_ASYNC = 2;
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.OAAnalyzerTitle), Resources.ResourceManager, typeof (Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.OAAnalyzerMessageFormat), Resources.ResourceManager, typeof (Resources));
        private static readonly LocalizableString MessageFormatNestedAsync = new LocalizableResourceString(nameof(Resources.OAAnalyzerMessageFormatNestedAsync), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.OAAnalyzerDescription), Resources.ResourceManager, typeof (Resources));
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true, Description);
        private static readonly DiagnosticDescriptor RuleNestedAsync = new DiagnosticDescriptor(DiagnosticIdNestedAsync, Title, MessageFormatNestedAsync, Category, DiagnosticSeverity.Warning, true, Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule, RuleNestedAsync);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationAction(CheckForPrivateAsyncs);
        }

        private static async void CheckForPrivateAsyncs(CompilationAnalysisContext context)
        {
            var solutionModel = await SolutionRepresentationFactory.Create(context.Compilation);

            foreach (var clazz in solutionModel.Classes)
            {
                foreach (var method in clazz.Methods)
                {
                    CheckForPrivateAsync(method, context);
                    if (CheckForNestedAsync(method, context, 0))
                    {
                        var diagnostic = Diagnostic.Create(RuleNestedAsync, method.MethodImplementation.GetLocation(), MAX_DEPTH_ASYNC+1);
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }

        private static List<InvocationExpressionRepresentation> AllInvocations(MethodRepresentation method)
        {
            if(method.Blocks.Count == 0) return new List<InvocationExpressionRepresentation>();

            List<InvocationExpressionRepresentation> expressionRepresentations = new List<InvocationExpressionRepresentation>();
            foreach (var block in method.Blocks)
            {
                expressionRepresentations.AddRange(block.InvocationExpressions);
                expressionRepresentations.AddRange(AllInvocations(block.Blocks.ToList()));
            }

            return expressionRepresentations;
        }

        private static List<InvocationExpressionRepresentation> AllInvocations(ICollection<IBody> blocks)
        {
            List<InvocationExpressionRepresentation> expressionRepresentations = new List<InvocationExpressionRepresentation>();
            foreach (var block in blocks)
            {
                expressionRepresentations.AddRange(block.InvocationExpressions);
                expressionRepresentations.AddRange(AllInvocations(block.Blocks.ToList()));
            }
            return expressionRepresentations;
        }

        private static bool CheckForNestedAsync(MethodRepresentation method, CompilationAnalysisContext context, int counter)
        {
            var symbol = (IMethodSymbol)context.Compilation.GetSemanticModel(method.MethodImplementation.SyntaxTree).GetDeclaredSymbol(method.MethodImplementation);
            
            if (symbol.IsAsync)
            {
                if (counter >= MAX_DEPTH_ASYNC)
                {
                    return true;
                }
                var invocations = AllInvocations(method);
                counter++;
                foreach (var invocation in invocations)
                {
                    if (CheckForNestedAsync(invocation, context, counter))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool CheckForNestedAsync(InvocationExpressionRepresentation invocation, CompilationAnalysisContext context, int counter)
        {
            if (invocation.InvokedImplementation is MethodRepresentation)
            {
                var method = ((MethodRepresentation) invocation.InvokedImplementation);
                var symbol = (IMethodSymbol)context.Compilation.GetSemanticModel(method.MethodImplementation.SyntaxTree).GetDeclaredSymbol(method.MethodImplementation);

                if (symbol.IsAsync)
                {
                    if (counter >= MAX_DEPTH_ASYNC)
                    {
                        return true;
                    }
                    counter++;
                    var invocations = AllInvocations(method);
                    foreach (var inv in invocations)
                    {
                        if (inv.InvokedImplementation is MethodRepresentation)
                        {
                            if (CheckForNestedAsync((MethodRepresentation) inv.InvokedImplementation, context, counter))
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        private static void CheckForPrivateAsync(MethodRepresentation method, CompilationAnalysisContext context)
        {
            var symbol = (IMethodSymbol)context.Compilation.GetSemanticModel(method.MethodImplementation.SyntaxTree).GetDeclaredSymbol(method.MethodImplementation);

            if(symbol.IsAsync && symbol.DeclaredAccessibility != Accessibility.Public)
            {
                var diagnostic = Diagnostic.Create(Rule, method.MethodImplementation.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }
        
    }
}