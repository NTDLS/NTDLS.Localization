using System.Globalization;

namespace NTDLS.Localization
{
    /// <summary>
    /// An <see cref="ILocalizer"/> that does not translate: it returns the English text (with any arguments put in). For libraries to use
    /// when they are not given a localizer, and for tests.
    /// </summary>
    public sealed class NullLocalizer
        : ILocalizer
    {
        /// <summary>
        /// Gets the singleton NullLocalizer instance used when localization is not required.
        /// </summary>
        /// <remarks>Shared, immutable instance to avoid repeated allocations; safe for concurrent
        /// use.</remarks>
        public static NullLocalizer Instance { get; } = new();

        /// <summary>
        /// Gets the culture used for resource lookups and user interface formatting.
        /// </summary>
        /// <remarks>Reflects CultureInfo.CurrentUICulture and may differ from CultureInfo.CurrentCulture;
        /// intended for localization and resource fallback.</remarks>
        public CultureInfo EffectiveCulture => CultureInfo.CurrentUICulture;

        /// <summary>
        /// Retrieves the localized string for the specified resource key.
        /// </summary>
        /// <param name="key">The resource key identifying the text to localize.</param>
        /// <returns>The localized string for the specified key, or the original key if no localization is available.</returns>
        public string Localize(string key) => key;

        /// <summary>
        /// Formats a composite format string with the specified arguments and returns the original key if formatting
        /// fails.
        /// </summary>
        /// <remarks>Catches FormatException and returns the key when formatting fails; no additional
        /// localization lookup is performed.</remarks>
        /// <param name="key">Composite format string to be processed; returned unchanged if formatting fails.</param>
        /// <param name="args">Objects to format into the composite format string.</param>
        /// <returns>The formatted string, or the original key if a FormatException is thrown during formatting.</returns>
        public string Localize(string key, params object[] args)
        {
            try
            {
                return string.Format(key, args);
            }
            catch (FormatException)
            {
                return key;
            }
        }
    }
}
