namespace NTDLS.Localization
{
    /// <summary>
    /// Where the package reports problems (a missing translation database, a translation with a broken placeholder). It writes nothing
    /// itself; set these to send the messages to the application's log. Reports can come from any thread.
    /// </summary>
    public static class LocalizationDiagnostics
    {
        public static Action<string, Exception?>? OnWarning { get; set; }
        public static Action<string, Exception?>? OnError { get; set; }

        internal static void Warning(string message, Exception? exception = null) => Report(OnWarning, message, exception);
        internal static void Error(string message, Exception? exception = null) => Report(OnError, message, exception);

        private static void Report(Action<string, Exception?>? target, string message, Exception? exception)
        {
            try
            {
                target?.Invoke(message, exception);
            }
            catch
            {
                //Reporting must never be the reason something fails.
            }
        }
    }
}
