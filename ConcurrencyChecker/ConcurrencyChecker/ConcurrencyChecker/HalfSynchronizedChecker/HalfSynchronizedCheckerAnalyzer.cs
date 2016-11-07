using System.Collections.Immutable;
using ConcurrencyAnalyzer.Diagnostics;
using ConcurrencyAnalyzer.Reporters.HalfSynchronizedReporter;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ConcurrencyChecker.HalfSynchronizedChecker
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class HalfSynchronizedCheckerAnalyzer : BaseAnalyzer
    {

        private static readonly DiagnosticDescriptor RuleHalfSynchronized = new DiagnosticDescriptor(HalfSynchronizedReporter.HalfSynchronizedChildDiagnosticId, HalfSynchronizedReporter.Title, HalfSynchronizedReporter.MessageFormatHalfSynchronized, HalfSynchronizedReporter.Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: HalfSynchronizedReporter.Description);
        private static readonly DiagnosticDescriptor RuleUnsynchronizedProperty = new DiagnosticDescriptor(HalfSynchronizedReporter.UnsynchronizedPropertyId, HalfSynchronizedReporter.Title, HalfSynchronizedReporter.MessageFormatUnsychronizedProperty, HalfSynchronizedReporter.Category, DiagnosticSeverity.Warning, isEnabledByDefault:true, description: HalfSynchronizedReporter.Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(RuleHalfSynchronized, RuleUnsynchronizedProperty);

        protected override Smell SelectSmell()
        {
            return Smell.HalfSynchronized;
        }
    }
}
