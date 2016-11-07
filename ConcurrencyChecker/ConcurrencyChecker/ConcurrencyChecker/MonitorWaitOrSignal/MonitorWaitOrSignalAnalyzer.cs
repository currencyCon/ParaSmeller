using System.Collections.Immutable;
using ConcurrencyAnalyzer.Diagnostics;
using ConcurrencyAnalyzer.Reporters.MonitorOrWaitSignalReporter;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ConcurrencyChecker.MonitorWaitOrSignal
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MonitorWaitOrSignalAnalyzer : BaseAnalyzer
    {

        private static readonly DiagnosticDescriptor MonitorIfRule = new DiagnosticDescriptor(MonitorOrWaitSignalReporter.MonitorIfConditionDiagnosticId, MonitorOrWaitSignalReporter.Title, MonitorOrWaitSignalReporter.MessageFormatIf, MonitorOrWaitSignalReporter.Category, DiagnosticSeverity.Warning, true, MonitorOrWaitSignalReporter.Description);
        private static readonly DiagnosticDescriptor MonitorPulseRule = new DiagnosticDescriptor(MonitorOrWaitSignalReporter.MonitorPulseDiagnosticId, MonitorOrWaitSignalReporter.Title, MonitorOrWaitSignalReporter.MessageFormatPulse, MonitorOrWaitSignalReporter.Category, DiagnosticSeverity.Warning, true, MonitorOrWaitSignalReporter.Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(MonitorIfRule, MonitorPulseRule);

        protected override Smell SelectSmell()
        {
            return Smell.MonitorWaitOrSignal;
        }
    }
}