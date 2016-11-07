using System.Linq;
using ConcurrencyAnalyzer.Representation;
using ConcurrencyAnalyzer.RepresentationExtensions;
using Microsoft.CodeAnalysis;
using Diagnostic = ConcurrencyAnalyzer.Diagnostics.Diagnostic;

namespace ConcurrencyAnalyzer.Reporters
{
    public class TentativelyResourceReferenceReporter: BaseReporter
    {
        public const string Category = "Synchronization";
        public const string DiagnosticId = "TRR001";

        private const string MonitorWait = "System.Threading.Monitor.Wait";
        private const string MonitorTryEnter = "System.Threading.Monitor.TryEnter";
        private const string WaitHandleWaitOne = "System.Threading.WaitHandle.WaitOne";
        private const string WaitHandleWaitAll = "System.Threading.WaitHandle.WaitAll";
        private const string WaitHandleWaitAny = "System.Threading.WaitHandle.WaitAny";
        private const string SpinLockTryEnter = "System.Threading.SpinLock.TryEnter";
        private const string BarrierSignalAndWait = "System.Threading.Barrier.SignalAndWait";

        private static readonly string[] TimeoutTypes = { "TimeSpan", "Int32" };
        private static readonly string[] NotAllowedApis = { MonitorWait, WaitHandleWaitOne, WaitHandleWaitAll, WaitHandleWaitAny, MonitorTryEnter, SpinLockTryEnter, BarrierSignalAndWait };

        public static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.TRRAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        public static readonly LocalizableString MessageFormatTentativelyResourceReference = new LocalizableResourceString(nameof(Resources.TRRAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        public static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.TRRAnalyzerDescription), Resources.ResourceManager, typeof(Resources));

        private void CheckForNotAllowedApiUsages(IMember member)
        {
            var invocationsToReport = member.Blocks.ToList()[0].InvocationExpressions.Where(e => NotAllowedApis.Contains(e.OriginalDefinition));
            foreach (var invocationToReport in invocationsToReport)
            {
                var symbol = invocationToReport.GetMethodSymbol(member.ContainingClass.SemanticModel);

                if (ContainsTimeout(symbol))
                {
                    Reports.Add(ReportTimeoutUsage(invocationToReport.Implementation));
                }
            }
        }

        private static bool ContainsTimeout(IMethodSymbol methodSymbol)
        {
            var parameters = methodSymbol.Parameters;
            foreach (var parameter in parameters)
            {
                if (TimeoutTypes.Contains(parameter.Type.Name))
                {
                    return true;
                }
            }
            return false;
        }

        private static Diagnostic ReportTimeoutUsage(SyntaxNode syntaxnode)
        {
            return new Diagnostic(DiagnosticId, Title, MessageFormatTentativelyResourceReference, Description, Category, syntaxnode.GetLocation());
        }

        public override void Register()
        {
            RegisterMethodReport(CheckForNotAllowedApiUsages);
        }
    }
}
