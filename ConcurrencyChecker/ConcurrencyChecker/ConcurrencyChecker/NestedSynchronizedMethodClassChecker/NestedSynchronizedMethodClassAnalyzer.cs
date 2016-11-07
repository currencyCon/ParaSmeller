using System.Collections.Immutable;
using ConcurrencyAnalyzer.Diagnostics;
using ConcurrencyAnalyzer.Reporters.NestedSynchronizedMethodClassReporter;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;


namespace ConcurrencyChecker.NestedSynchronizedMethodClassChecker
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class NestedSynchronizedMethodClassAnalyzer : BaseAnalyzer
    {
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(NestedSynchronizedMethodClassReporter.NestedLockingDiagnosticId, NestedSynchronizedMethodClassReporter.Title, NestedSynchronizedMethodClassReporter.MessageFormat, NestedSynchronizedMethodClassReporter.Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: NestedSynchronizedMethodClassReporter.Description);
        private static readonly DiagnosticDescriptor Rule2 = new DiagnosticDescriptor(NestedSynchronizedMethodClassReporter.NestedLockingDiagnosticId2, NestedSynchronizedMethodClassReporter.Title, NestedSynchronizedMethodClassReporter.MessageFormat, NestedSynchronizedMethodClassReporter.Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: NestedSynchronizedMethodClassReporter.Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule, Rule2);


        protected override Smell SelectSmell()
        {
            return Smell.NestedSynchronization;
        }
    }
}
