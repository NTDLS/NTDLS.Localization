using System.Globalization;

namespace NTDLS.Localization
{
    /// <summary>
    /// An <see cref="ILocalizer"/> that does not translate: it returns the English text (with any arguments put in). For libraries to use
    /// when they are not given a localizer, and for tests.
    /// </summary>
    public sealed class NullLocalizer : ILocalizer
    {
        public static NullLocalizer Instance { get; } = new();

        public CultureInfo EffectiveCulture => CultureInfo.CurrentUICulture;

        public string Localize(string key) => key;

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
