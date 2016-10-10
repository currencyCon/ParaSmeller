namespace ConcurrencyAnalyzer
{
    public class A
    {
        public B B { get; set; }

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
            a.DoAStuff();
        }
    }
}
