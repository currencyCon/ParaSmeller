﻿namespace ConcurrencyAnalyzer
{
    public class A
    {
        public B B { get; set; }

        private int _a;
        public int AProperty
        {
            get
            {
                lock (this)
                {
                    return _a;
                }
            }
            set
            {
                lock (this)
                {
                    _a = value + B.BProperty;
                }
            }
        }
        public void DoAStuff()
        {
            lock (this)
            {
                B.DoBStuff();
            }
        }
    }

    public class B
    {
        public A A { get; set; }

        private int _b;
        public int BProperty
        {
            get
            {
                lock (this)
                {
                    return _b;
                }
            }
            set
            {
                lock (this)
                {
                    _b = value + A.AProperty;
                }
            }
        }


        public void DoBStuff()
        {
            lock (this)
            {
                A.DoAStuff();
            }
        }

    }

    public class Maiclass
    {
        public static void Main()
        {
            var a = new A();
            var b = new B();
            a.B = b;
            b.A = a;
            var x = a.AProperty;
            a.DoAStuff();
        }
    }
}
