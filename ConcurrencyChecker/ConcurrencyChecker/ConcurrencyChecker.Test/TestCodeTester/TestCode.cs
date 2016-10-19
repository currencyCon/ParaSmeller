namespace Test
{
    public class LockObject
    {
        
    }
    public class TestProgram
    {
        private LockObject LockObject { get; }

        public TestProgram(LockObject lockObject)
        {
            LockObject = lockObject;
        }
        public int z { get; set; }

        public void m()
        {
            lock (LockObject)
            {
                z = 2;
            }
        }

        public void m2()
        {
            lock (LockObject)
            {
                z = 3;
            }
        }
    }
}