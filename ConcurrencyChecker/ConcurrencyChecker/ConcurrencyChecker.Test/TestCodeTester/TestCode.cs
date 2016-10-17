using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConcurrencyChecker.Test.TestCodeTester
{
    public class TestCode
    {
        public static void X()
        {
            Thread.Sleep(500000000);
            Console.WriteLine("Huhu");
        }

        public static void Main()
        {
            var z = 3;
            var x = Task.Run(() => X());
            Console.WriteLine("Lol");
        }

    }
}
