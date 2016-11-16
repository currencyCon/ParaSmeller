

using System.Linq;
using ConcurrencyAnalyzer.Representation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Diagnostic = ConcurrencyAnalyzer.Diagnostics.Diagnostic;

namespace ConcurrencyAnalyzer.Reporters
{
    public class WaitingConditionsTasksReporter: BaseReporter
    {
        public const string Category = "Synchronization";
        public const string WaitingConditionsTasksDiagnosticId = "WCT001";
        public static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.WCTAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        public static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.WCTAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        public static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.WCTAnalyzerDescription), Resources.ResourceManager, typeof(Resources));

        private  void CheckForLockUsage(Member member)
        {
            var invocationsInThreads = member.GetAllInvocations().Where(e => e.IsInvokedInTask);
            foreach (var invocationExpressionRepresentation in invocationsInThreads)
            {
                if (invocationExpressionRepresentation.InvokedImplementations.Count > 0 && invocationExpressionRepresentation.InvokedImplementations.SelectMany(s => s.GetChildren<LockStatementSyntax>()).Any())
                {
                    Reports.Add(ReportSyncMechanismInTask(invocationExpressionRepresentation.Implementation));
                }
            }
        }

        private static Diagnostic ReportSyncMechanismInTask(SyntaxNode syntaxnode)
        {
            return new Diagnostic(WaitingConditionsTasksDiagnosticId, Title, MessageFormat, Description, Category, syntaxnode.GetLocation());
        }

        protected override void Register()
        {
            RegisterMemberReport(CheckForLockUsage);
        }

        
    }
}
