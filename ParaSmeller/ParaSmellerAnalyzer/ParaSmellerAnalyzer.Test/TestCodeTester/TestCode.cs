namespace bla
{
    class Program
    {
        private int _z;

        public int z
        {
            get
            {
                var x = 3;
                lock (this)
                {
                    if (_z > 4)
                    {
                        x = _z;
                    }
                }
                return x;
            }

            set
            {
                lock (this)
                {
                    _z = value;
                }
            }
        }

        public void DoNothing()
        {
            
        }
        public void m()
        {
            lock (this)
            {
                z = 2;
                DoNothing();
            }
        }
    }
}