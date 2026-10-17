using System.Globalization;

namespace NTDLS.Localization
{
    /// <summary>
    /// Looks up display text. The English text is the key, and it is returned unchanged when there is no translation, so code can be
    /// written (and tested) in English and a missing translation only ever costs the translation, never the text.
    ///
    /// This is what to pass to other libraries, so that they can show text in the application's language without depending
    /// on how the application stores its translations: <c>new MyLibrary(Localizer.Default)</c>.
    /// </summary>
    public interface ILocalizer
    {
        /// <summary>
        /// The language that text is translated to: the chosen language, or the Windows display language when none is chosen.
        /// </summary>
        CultureInfo EffectiveCulture { get; }

        /// <summary>
        /// Gets the text for a key in the current language, or the key itself when there is no translation.
        /// </summary>
        string Localize(string key);

        /// <summary>
        /// Gets the text for a key in the current language, with the arguments put into it ({0}, {1}, ...) like <see cref="string.Format(string, object[])"/>.
        /// The translation must keep the same placeholders as the key.
        /// </summary>
        string Localize(string key, params object[] args);
    }
}
