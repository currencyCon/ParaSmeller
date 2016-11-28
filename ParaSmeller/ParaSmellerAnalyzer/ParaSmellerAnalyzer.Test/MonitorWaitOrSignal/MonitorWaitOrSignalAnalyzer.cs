using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using ParaSmellerAnalyzer.Analyzer;
using ParaSmellerCore.Diagnostics;

namespace ParaSmeller.Test.MonitorWaitOrSignal
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MonitorWaitOrSignalAnalyzer : BaseAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rules.MonitorIfRule, Rules.MonitorPulseRule);

        protected override ICollection<Smell> SelectSmell()
        {
            return new List<Smell> { Smell.MonitorWaitOrSignal};
        }
    }
}