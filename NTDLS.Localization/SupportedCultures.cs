using System.Diagnostics.CodeAnalysis;

namespace NTDLS.Localization
{
    /// <summary>
    /// Contains a predefined collection of supported culture codes and their display names.
    /// </summary>
    /// <remarks>Initialized with a common set of language and region codes. Exposes a read-only collection
    /// via the Collection property and provides TryGetByCode to retrieve an entry by its code.</remarks>
    public class SupportedCultures
    {
        /// <summary>
        /// Gets the collection of CultureInfoSettings.
        /// </summary>
        /// <remarks>The list instance is exposed for modification (items can be added, removed, or
        /// updated); the setter is private so the reference cannot be replaced externally.</remarks>
        public IList<CultureInfoSettings> Collection { get; private set; }

        /// <summary>
        /// Tries to get a CultureInfoSettings from the collection by its code.
        /// </summary>
        /// <param name="code">The culture code to search for.</param>
        /// <param name="result">When the method returns true, contains the matching CultureInfoSettings; otherwise null.</param>
        /// <returns>True if a matching entry was found; otherwise false.</returns>
        public bool TryGetByCode(string code, [NotNullWhen(true)] out CultureInfoSettings? result)
        {
            result = Collection.FirstOrDefault(o => o.Code == code);
            return result != null;
        }

        /// <summary>
        /// Initializes a new SupportedCultures instance and populates Collection with predefined CultureInfoSettings
        /// for common languages and locales.
        /// </summary>
        /// <remarks>Populates Collection with language codes and display names (for example: en, ar,
        /// zh-Hans, zh-Hant, hi, ur) to provide a default set of supported cultures covering multiple scripts and
        /// regions.</remarks>
        public SupportedCultures()
        {
            Collection = new List<CultureInfoSettings>
            {
                new("en", "English"),
                new("ar", "Arabic"),
                new("az", "Azerbaijani"),
                new("be", "Belarusian"),
                new("bg", "Bulgarian"),
                new("cs", "Czech"),
                new("da", "Danish"),
                new("de", "German"),
                new("el", "Greek"),
                new("es", "Spanish"),
                new("et", "Estonian"),
                new("fa", "Persian"),
                new("fi", "Finnish"),
                new("fr", "French"),
                new("he", "Hebrew"),
                new("hr", "Croatian"),
                new("hu", "Hungarian"),
                new("id", "Indonesian"),
                new("is", "Icelandic"),
                new("it", "Italian"),
                new("ja", "Japanese"),
                new("ka", "Georgian"),
                new("kk", "Kazakh"),
                new("ko", "Korean"),
                new("lt", "Lithuanian"),
                new("lv", "Latvian"),
                new("ms", "Malay"),
                new("nl", "Dutch"),
                new("nn", "Norwegian"),
                new("nb", "Bokmål"),
                new("pl", "Polish"),
                new("pt", "Portuguese"),
                new("ro", "Romanian"),
                new("ru", "Russian"),
                new("sk", "Slovak"),
                new("sl", "Slovenian"),
                new("sr", "Serbian"),
                new("sv", "Swedish"),
                new("th", "Thai"),
                new("tr", "Turkish"),
                new("uk", "Ukrainian"),
                new("vi", "Vietnamese"),
                new("zh-Hans", "Chinese simplified"),
                new("zh-Hant", "Chinese traditional"),
                new("bn", "Bengali"),
                new("hi", "Hindi"),
                new("ur", "Urdu")
            };
        }
    }
}
