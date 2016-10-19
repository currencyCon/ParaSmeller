using System;
using System.Threading;
using System.Threading.Tasks;

namespace Test
{
    public class TestCode
    {
        public static void X()
        {
            Thread.Sleep(5);
            Console.WriteLine("Huhu");
        }

        public static void Main()
        {
            var z = 3;
            var x = Task.Run(() => X());
            var w = new Waiter();
            w.Await(x);
            Console.WriteLine("Lol");
        }

        private static void Wait(Task task)
        {
            task.Wait();
        }
    }

    public class Waiter
    {
        public void Await(Task task)
        {
            task.Wait();
        }
    }
}