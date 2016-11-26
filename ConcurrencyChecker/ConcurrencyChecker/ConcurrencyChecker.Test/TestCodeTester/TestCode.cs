namespace Test
{
    class TestProgram
    {
        public int z
        {
            get
            {
                var x = 2;
                return x;
            }
        }

        public void m()
        {
            lock (this)
            {
                var x = z + 1;
            }
        }
    }
}