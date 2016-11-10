using System.Threading.Tasks;

namespace ConcurrencyChecker.Test.TestCodeTester
{
    public class SynchronizedThread
    {
        public static object LockObject = new object();
        public static void DoTask()
        {
            lock (LockObject)
            {
                var c = 2;
            }
        }

        public static void Main()
        {
            Task.Run(()=> DoTask());
        }
    }
}