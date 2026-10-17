namespace NTDLS.Localization
{
    /// <summary>
    /// Where the package reports problems (a missing translation database, a translation with a broken placeholder). It writes nothing
    /// itself; set these to send the messages to the application's log. Reports can come from any thread.
    /// </summary>
    public static class LocalizationDiagnostics
    {
        /// <summary>
        /// Invoked for warning notifications and provided with a message and an optional exception.
        /// </summary>
        /// <remarks>May be set to a delegate to handle warnings; null if no handler is registered.
        /// Handlers should be short‑running and handle their own exceptions.</remarks>
        public static Action<string, Exception?>? OnWarning { get; set; }

        /// <summary>
        /// Invoked to report errors, receiving a message and an optional exception.
        /// </summary>
        /// <remarks>Assign a delegate to handle or log errors. May be null to disable error reporting.
        /// Handlers should avoid throwing exceptions. Invocation occurs on the context that detects the
        /// error.</remarks>
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
