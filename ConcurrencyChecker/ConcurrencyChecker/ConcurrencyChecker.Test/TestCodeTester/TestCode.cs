

namespace Test
{
    public class VolatileTest<TVol> where TVol : class
    {
        public volatile TVol VolatileElement;

        public void Test(TVol volatileElement)
        {
            VolatileElement = volatileElement;
        }
    }
}