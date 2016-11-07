using System.Collections.Immutable;
using ConcurrencyAnalyzer.Diagnostics;
using ConcurrencyAnalyzer.Reporters.ExplicitThreadsReporter;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ConcurrencyChecker.ExplicitThreadsChecker
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ExplicitThreadsAnalyzer : BaseAnalyzer
    {
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(ExplicitThreadsReporter.DiagnosticId, ExplicitThreadsReporter.Title, ExplicitThreadsReporter.MessageFormat, ExplicitThreadsReporter.Category, DiagnosticSeverity.Warning, true, ExplicitThreadsReporter.Description);
        
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        protected override Smell SelectSmell()
        {
            return Smell.ExplicitThreads;
        }
    }
}