

using System.Linq;
using ConcurrencyAnalyzer.Representation;
using ConcurrencyAnalyzer.RepresentationExtensions;
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
        public static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.WaitingConditionsTasksAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        public static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.WCTAnalyzerDescription), Resources.ResourceManager, typeof(Resources));

        private  void CheckForLockUsage(IMember member)
        {
            var invocationsInThreads = member.GetAllInvocations().Where(e => e.IsInvokedInTask);
            foreach (var invocationExpressionRepresentation in invocationsInThreads)
            {
                if (invocationExpressionRepresentation.InvokedImplementation != null && invocationExpressionRepresentation.InvokedImplementation.GetChildren<LockStatementSyntax>().Any())
                {
                    Reports.Add(ReportSyncMechanismInTask(invocationExpressionRepresentation.Implementation));
                }
            }
        }

        private static Diagnostic ReportSyncMechanismInTask(SyntaxNode syntaxnode)
        {
            return new Diagnostic(WaitingConditionsTasksDiagnosticId, Title, MessageFormat, Description, Category, syntaxnode.GetLocation());
        }
        public override void Register()
        {
            RegisterMemberReport(CheckForLockUsage);
        }

        
    }
}
