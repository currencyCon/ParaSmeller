using System;
using System.Threading;
using System.Threading.Tasks;

namespace bla
{
    public class Program
    {
        public static void X()
        {
            Thread.Sleep(5);
            Console.WriteLine("Huhu");
        }

        public static async Task Main()
        {
            var z = 3;
            await Task.Run(() => X());
            Console.WriteLine("Lol");
        }
    }
}