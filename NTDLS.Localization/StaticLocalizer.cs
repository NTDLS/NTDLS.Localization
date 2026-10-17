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
        public static string Localize(string key) => Localizer.Default.Localize(key);

        public static string Localize(string key, params object[] args) => Localizer.Default.Localize(key, args);
    }
}
