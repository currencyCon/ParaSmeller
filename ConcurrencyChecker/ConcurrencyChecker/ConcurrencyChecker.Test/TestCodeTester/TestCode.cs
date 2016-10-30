

using System.Threading;
using System.Threading.Tasks;

namespace Test
{
    public class YieldTest
    {
        private static void DoLongWork()
        {
            for (var i = 0; i < 1000; i++)
            {
                DoHardStuff();
                Thread.Yield();
            }
        }

        public static void Main()
        {
            var x = Task.Run(() => DoLongWork());
            x.Wait();
        }

        private static void DoHardStuff()
        {
            Thread.Sleep(100);
        }
    }
}