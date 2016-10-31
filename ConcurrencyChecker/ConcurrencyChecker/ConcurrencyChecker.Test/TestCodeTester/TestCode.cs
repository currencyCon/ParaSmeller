namespace ConcurrencyChecker.Test.TestCodeTester
{
    public class SynchronizedFinalizer
    {
        public static int Counter;
        private readonly object LockObjectA = new object();
        private readonly object LockObjectB = new object();

        public SynchronizedFinalizer()
        {
            lock (LockObjectA)
            {
                Counter++;
            }
        }

        ~SynchronizedFinalizer()
        {
            lock (LockObjectB)
            {
                Counter--;
            }
        }
    }
}