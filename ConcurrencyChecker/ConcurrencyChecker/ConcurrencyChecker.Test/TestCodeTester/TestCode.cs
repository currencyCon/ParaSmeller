using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

class SpinLockDemo2
{
    const int N = 100000;
    static readonly Queue<Data> Queue = new Queue<Data>();
    private static readonly object Lock = new object();
    private static SpinLock _spinlock = new SpinLock();

    private class Data
    {
        public string Name { get; set; }
        public double Number { get; set; }
    }
    static void Main(string[] args)
    {

        // First use a standard lock for comparison purposes.
        UseLock();
        Queue.Clear();
        UseSpinLock();

        Console.WriteLine(@"Press a key");
        Console.ReadKey();

    }

    private static void UpdateWithSpinLock(Data d, int i)
    {
        var lockTaken = false;
        try
        {
            _spinlock.Enter(ref lockTaken);
            Queue.Enqueue(d);
        }
        finally
        {
            if (lockTaken) _spinlock.Exit(false);
        }
    }

    private static void UseSpinLock()
    {

        var sw = Stopwatch.StartNew();

        Parallel.Invoke(
                () => {
                    for (var i = 0; i < N; i++)
                    {
                        UpdateWithSpinLock(new Data { Name = i.ToString(), Number = i }, i);
                    }
                },
                () => {
                    for (var i = 0; i < N; i++)
                    {
                        UpdateWithSpinLock(new Data { Name = i.ToString(), Number = i }, i);
                    }
                }
            );
        sw.Stop();
        Console.WriteLine(@"elapsed ms with spinlock: {0}", sw.ElapsedMilliseconds);
    }

    private static void UpdateWithLock(Data d)
    {
        lock (Lock)
        {
            Queue.Enqueue(d);
        }
    }

    private static void UseLock()
    {
        var sw = Stopwatch.StartNew();

        Parallel.Invoke(
                () => {
                    for (var i = 0; i < N; i++)
                    {
                        UpdateWithLock(new Data { Name = i.ToString(), Number = i });
                    }
                },
                () => {
                    for (var i = 0; i < N; i++)
                    {
                        UpdateWithLock(new Data { Name = i.ToString(), Number = i });
                    }
                }
            );
        sw.Stop();
        Console.WriteLine(@"elapsed ms with lock: {0}", sw.ElapsedMilliseconds);
    }
}