namespace ParaSmellerCore
{
    public static class Logger
    {
        public static void Debug(string log)
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine(log);
#endif
        }
    }
}