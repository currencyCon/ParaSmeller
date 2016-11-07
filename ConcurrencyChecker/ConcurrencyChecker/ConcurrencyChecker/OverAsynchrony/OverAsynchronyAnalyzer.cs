using System.Collections.Immutable;
using ConcurrencyAnalyzer.Diagnostics;
using ConcurrencyAnalyzer.Reporters.OverAsynchronyReporter;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ConcurrencyChecker.OverAsynchrony
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class OverAsynchronyAnalyzer : BaseAnalyzer
    {

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(OverAsynchronyReporter.DiagnosticId, OverAsynchronyReporter.Title, OverAsynchronyReporter.MessageFormat, OverAsynchronyReporter.Category, DiagnosticSeverity.Warning, true, OverAsynchronyReporter.Description);
        private static readonly DiagnosticDescriptor RuleNestedAsync = new DiagnosticDescriptor(OverAsynchronyReporter.DiagnosticIdNestedAsync, OverAsynchronyReporter.Title, OverAsynchronyReporter.MessageFormatNestedAsync, OverAsynchronyReporter.Category, DiagnosticSeverity.Warning, true, OverAsynchronyReporter.Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule, RuleNestedAsync);


        protected override Smell SelectSmell()
        {
            return Smell.OverAsynchrony;
        }
    }
}