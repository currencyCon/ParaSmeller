namespace ConcurrencyAnalyzer.Diagnostics
{
    public enum Smell
    {
        PrimitiveSynchronization,
        ExplicitThreads,
        Finalizer,
        FireAndForget,
        HalfSynchronized,
        MonitorWaitOrSignal,
        NestedSynchronization,
        OverAsynchrony,
        TenativelyRessource,
        Tapir
    }
}
