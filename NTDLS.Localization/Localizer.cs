using System.Collections.Concurrent;
using System.Globalization;

namespace NTDLS.Localization
{
    /// <summary>
    /// Looks up display text in the translation database. The English text in the source code is the key:
    /// <c>Localize("Search")</c> returns the translation of "Search" for the current UI language, and returns "Search" itself
    /// when there is no translation, so a missing database only ever costs the translation, never the text.
    ///
    /// The database is a SQLite file with one row per English phrase and one column per language. See <see cref="TranslationDatabase"/>.
    /// Pass it to other libraries as an <see cref="ILocalizer"/>.
    /// </summary>
    public class Localizer : ILocalizer
    {
        private static Localizer? _default;

        /// <summary>
        /// Where the application's translation database is: Translations\Translations.db next to the program.
        /// </summary>
        public static string DefaultDatabaseFile => Path.Combine(AppContext.BaseDirectory, "Translations", "Translations.db");

        /// <summary>
        /// The localizer the application uses (and <see cref="StaticLocalizer"/> uses). Created on first use from <see cref="DefaultDatabaseFile"/>.
        /// </summary>
        public static Localizer Default
        {
            get => _default ??= new Localizer(DefaultDatabaseFile);
            set => _default = value;
        }

        private readonly TranslationDatabase _database;
        private readonly ConcurrentDictionary<string, bool> _reported = new();

        /// <summary>
        /// Initializes a new instance of Localizer that loads translations from the specified database file.
        /// </summary>
        /// <remarks>The path is forwarded to TranslationDatabase; exceptions thrown by that constructor
        /// are propagated.</remarks>
        /// <param name="databaseFile">Path to the translation database file.</param>
        public Localizer(string databaseFile)
        {
            _database = new TranslationDatabase(databaseFile);
        }

        /// <summary>
        /// The language to translate to. When null the current UI culture (the Windows display language) is used.
        /// </summary>
        public CultureInfo? Culture { get; set; }

        /// <summary>
        /// Chooses the language: a culture name such as "de" or "pt-BR", or null, empty or "auto" to follow Windows.
        /// </summary>
        public void SetLanguage(string? cultureName)
        {
            if (string.IsNullOrWhiteSpace(cultureName) || cultureName.Trim().Equals("auto", StringComparison.OrdinalIgnoreCase))
            {
                Culture = null;
                return;
            }

            try
            {
                Culture = CultureInfo.GetCultureInfo(cultureName.Trim(), predefinedOnly: true);
            }
            catch (CultureNotFoundException ex)
            {
                LocalizationDiagnostics.Warning($"\"{cultureName}\" is not a language that can be used. The Windows language will be used.", ex);
                Culture = null;
            }
        }

        /// <inheritdoc />
        public CultureInfo EffectiveCulture => Culture ?? CultureInfo.CurrentUICulture;

        /// <inheritdoc />
        public string Localize(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return key;
            }

            return _database.TryGetTranslation(EffectiveCulture, key, out var translation) ? translation : key;
        }

        /// <inheritdoc />
        public string Localize(string key, params object[] args)
        {
            if (string.IsNullOrEmpty(key))
            {
                return key;
            }

            var text = Localize(key);
            try
            {
                return string.Format(text, args);
            }
            catch (FormatException ex)
            {
                if (_reported.TryAdd(key, true))
                {
                    LocalizationDiagnostics.Warning($"The translation for \"{key}\" cannot be formatted, so the original text is used.", ex);
                }

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
}
