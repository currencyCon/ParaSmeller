using System.Diagnostics;

namespace ConcurrencyAnalyzer
{
    public static class Logger
    {
        public static void DebugLog(string log)
        {
#if DEBUG
            Debug.WriteLine(log);
#endif
        }
    }
}