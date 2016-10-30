using System;
using System.Threading;

namespace ConcurrencyChecker.Test.TestCodeTester
{
    public class Foo
    {
        int _answer;
        bool _complete;

        public void A()
        {
            _answer = 123;
            Thread.MemoryBarrier();
            _complete = true;
            Thread.MemoryBarrier();    
        }

        public  void B()
        {
            Thread.MemoryBarrier();
            if (_complete)
            {
                Thread.MemoryBarrier();
                Console.WriteLine(_answer);
            }
        }
    }
}