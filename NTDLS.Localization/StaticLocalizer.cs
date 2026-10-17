namespace NTDLS.Localization
{
    /// <summary>
    /// Short static helpers for the application's localizer. The project imports this class with a global "using static", so any
    /// code can simply write:
    ///
    ///     Text = Localize("Search");
    ///     MessageBox.Show(Localize("Found {0} files", count));
    ///
    /// The English text is the key: a SQLite database holds one column per language. These use <see cref="Localizer.Default"/>.
    /// Add  &lt;Using Include="NTDLS.Localization.StaticLocalizer" Static="true" /&gt;  to the project to use them without a prefix.
    /// </summary>
    public static class StaticLocalizer
    {
        /// <summary>
        /// Retrieves a localized string for the specified resource key.
        /// </summary>
        /// <remarks>Resolves translations using Localizer.Default.</remarks>
        /// <param name="key">The resource key to look up.</param>
        /// <returns>The localized string for the specified key, or the key itself if no translation is available.</returns>
        public static string Localize(string key) => Localizer.Default.Localize(key);

        /// <summary>
        /// Gets a localized string for the specified resource key and formats it with the provided arguments.
        /// </summary>
        /// <remarks>Resolution is performed by Localizer.Default using the current localization
        /// configuration.</remarks>
        /// <param name="key">The resource key identifying the text to localize.</param>
        /// <param name="args">Optional format arguments applied to the localized string.</param>
        /// <returns>The localized string formatted with the specified arguments.</returns>
        public static string Localize(string key, params object[] args) => Localizer.Default.Localize(key, args);
    }
}
