using System.Collections.Immutable;
using ConcurrencyAnalyzer.Diagnostics;
using ConcurrencyAnalyzer.Reporters.FireAndForgetReporter;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ConcurrencyChecker.FireAndForgetChecker
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class FireAndForgetCheckerAnalyzer: BaseAnalyzer
    {

        private static readonly DiagnosticDescriptor RuleFireAndForgetCall = new DiagnosticDescriptor(FireAndForgetReporter.FireAndForgetCallId, FireAndForgetReporter.Title, FireAndForgetReporter.MessageFormatFireAndForghet, FireAndForgetReporter.Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: FireAndForgetReporter.Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(RuleFireAndForgetCall);

        protected override Smell SelectSmell()
        {
            return Smell.FireAndForget;
        }
    }
}
