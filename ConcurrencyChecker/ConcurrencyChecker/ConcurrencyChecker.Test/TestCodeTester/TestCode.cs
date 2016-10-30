using System.Threading;

namespace Test
{
    public class BancAcount
    {
        private int _balance;

        public void Withdraw(int amount)
        {
            var value = _balance;
            if (value >= _balance)
            {
                Interlocked.Add(ref _balance, -amount);
            }
        }
    }
}