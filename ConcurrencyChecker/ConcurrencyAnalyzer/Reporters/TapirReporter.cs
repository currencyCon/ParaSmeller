﻿using ConcurrencyAnalyzer.Diagnostics;
using ConcurrencyAnalyzer.Representation;
using Microsoft.CodeAnalysis;
using Diagnostic = ConcurrencyAnalyzer.Diagnostics.Diagnostic;

namespace ConcurrencyAnalyzer.Reporters
{
    public class TapirReporter:BaseReporter
    {
        public const string DiagnosticId = "TAPIR001";
        private const string TapirClass = "Tapir";

        public static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.TapirAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        public static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.TapirAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        public static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.TapirAnalyzerDescription), Resources.ResourceManager, typeof(Resources));

        protected override void Register()
        {
            RegisterClassReport(CheckClassForTapir);
        }

        private void CheckClassForTapir(ClassRepresentation clazz)
        {
            if (clazz.Implementation.Identifier.Text == TapirClass)
            {
                Reports.Add(new Diagnostic(DiagnosticId, Title, MessageFormat, Description, DiagnosticCategory.ParallelCorrectness,
                    clazz.Implementation.Identifier.GetLocation()));
            }
        }
    }
}
