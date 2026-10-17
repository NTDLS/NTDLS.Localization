using Microsoft.Data.Sqlite;
using System.Collections.Concurrent;
using System.Globalization;

namespace NTDLS.Localization
{
    /// <summary>
    /// Reads translations from a SQLite database. The table is called Translation. It has one row per English phrase and one
    /// column per language:
    ///
    ///   TranslationId  INTEGER PRIMARY KEY
    ///   Phrase   TEXT NOT NULL UNIQUE COLLATE NOCASE   (the English phrase as it is written in the source code: the key)
    ///   de, fr, ja, zh-Hans, pt-BR ...  TEXT           (one column per language, named by its culture code; empty means "not translated yet")
    ///   LastModified  TEXT                              (when the row was last changed, UTC; not read here)
    ///
    /// The database is only read, never changed. Each language is read once, the first time it is needed, and is then served from memory.
    /// </summary>
    public class TranslationDatabase
    {
        private const string TableName = "Translation";
        private const string SourceColumn = "Phrase";
        private const string LastModifiedColumn = "LastModified";

        private static readonly SupportedCultures _supportedCultures = new();

        private static readonly Dictionary<string, string> _alternateCodes = new(StringComparer.OrdinalIgnoreCase)
        {
            ["no"] = "nb"
        };

        private readonly string _databaseFile;
        private readonly bool _isAvailable;
        private readonly Lazy<HashSet<string>> _languageColumns;
        private readonly ConcurrentDictionary<string, Lazy<Dictionary<string, string>>> _languages = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Initializes a new TranslationDatabase instance for the specified SQLite file, stores the file path,
        /// determines availability without creating the file, logs a warning if the file is missing, and defers loading
        /// language columns with a lazy initializer.
        /// </summary>
        /// <remarks>Opening a nonexistent SQLite file would create an empty database, so the constructor
        /// checks File.Exists and sets availability accordingly; if the file is missing a warning is logged. Language
        /// columns are loaded on demand via a Lazy(HashSet of string) that calls ReadLanguageColumns.</remarks>
        /// <param name="databaseFile">Path to the SQLite translation database file.</param>
        public TranslationDatabase(string databaseFile)
        {
            _databaseFile = databaseFile;

            //Opening a SQLite file that does not exist would create an empty one, so check first.
            _isAvailable = File.Exists(databaseFile);
            if (!_isAvailable)
            {
                LocalizationDiagnostics.Warning($"The translation database \"{databaseFile}\" was not found. All text will be shown in English.");
            }

            _languageColumns = new Lazy<HashSet<string>>(ReadLanguageColumns);
        }

        private SqliteConnection Open()
        {
            //Read only and not pooled, so the file is never locked.
            var connection = new SqliteConnection(new SqliteConnectionStringBuilder
            {
                DataSource = _databaseFile,
                Mode = SqliteOpenMode.ReadOnly,
                Pooling = false
            }.ToString());

            connection.Open();
            return connection;
        }

        private HashSet<string> ReadLanguageColumns()
        {
            var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (!_isAvailable)
            {
                return columns;
            }

            try
            {
                using var connection = Open();
                using var command = connection.CreateCommand();
                command.CommandText = $"PRAGMA table_info(\"{TableName}\")";

                var all = new List<(string Name, bool IsPrimaryKey)>();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        all.Add((reader.GetString(1), reader.GetInt32(5) > 0));
                    }
                }


                //The row id column is found by being the primary key, not by its name: "id" (Indonesian) is a language, and SQLite
                //  column names are not case sensitive, so the row id column cannot be called "Id" in a database that has it.
                foreach (var (name, isPrimaryKey) in all)
                {
                    if (!isPrimaryKey
                        && !name.Equals(SourceColumn, StringComparison.OrdinalIgnoreCase)
                        && !name.Equals(LastModifiedColumn, StringComparison.OrdinalIgnoreCase))
                    {
                        columns.Add(name);
                    }
                }
            }
            catch (Exception ex)
            {
                LocalizationDiagnostics.Error($"Unable to read the languages from the translation database \"{_databaseFile}\".", ex);
            }

            return columns;
        }

        private Dictionary<string, string> Load(string column)
        {
            var phrases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                using var connection = Open();
                using var command = connection.CreateCommand();
                var quoted = "\"" + column.Replace("\"", "\"\"") + "\"";
                command.CommandText = $"SELECT {SourceColumn}, {quoted} FROM {TableName} WHERE {quoted} IS NOT NULL AND {quoted} <> ''";

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    phrases[reader.GetString(0)] = reader.GetString(1);
                }
            }
            catch (Exception ex)
            {
                LocalizationDiagnostics.Error($"Unable to read the \"{column}\" translations from \"{_databaseFile}\".", ex);
            }

            return phrases;
        }

        /// <summary>
        /// The columns that can supply text for a culture, most specific first: "de-AT" is followed by "de".
        /// </summary>
        private IEnumerable<string> FindColumns(CultureInfo culture)
        {
            for (var current = culture; !string.IsNullOrEmpty(current.Name); current = current.Parent)
            {
                var code = _alternateCodes.TryGetValue(current.Name, out var alternate) ? alternate : current.Name;

                //The column is named by the culture code. The older names ("German", "Chinese simplified") are still read, so a
                //  database that has not been migrated keeps working.
                var candidates = new List<string> { current.Name, code };
                if (_supportedCultures.TryGetByCode(code, out var supported))
                {
                    candidates.Add(supported.Name);
                }
                candidates.Add(current.EnglishName);

                foreach (var candidate in candidates)
                {
                    if (_languageColumns.Value.TryGetValue(candidate, out var column))
                    {
                        yield return column;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Attempts to retrieve a localized translation for the specified English phrase in the given culture.
        /// </summary>
        /// <remarks>Returns false when translations are unavailable or when the culture is English. The
        /// implementation searches configured language columns and lazily loads translation dictionaries.</remarks>
        /// <param name="culture">Target culture for the translation.</param>
        /// <param name="phrase">English phrase to translate.</param>
        /// <param name="translation">When the method returns, contains the localized translation if found; otherwise an empty string.</param>
        /// <returns>true if a translation was found and assigned to translation; otherwise false.</returns>
        public bool TryGetTranslation(CultureInfo culture, string phrase, out string translation)
        {
            translation = string.Empty;

            //The phrases are English, so English needs no lookup.
            if (!_isAvailable || culture.TwoLetterISOLanguageName == "en")
            {
                return false;
            }

            foreach (var column in FindColumns(culture))
            {
                var phrases = _languages.GetOrAdd(column, c => new Lazy<Dictionary<string, string>>(() => Load(c))).Value;
                if (phrases.TryGetValue(phrase, out var found))
                {
                    translation = found;
                    return true;
                }
            }

            return false;
        }
    }
}
