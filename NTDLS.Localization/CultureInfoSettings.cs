using System.Globalization;

namespace NTDLS.Localization
{
    /// <summary>
    /// Represents culture-related settings, including the culture code, display name, and the associated CultureInfo.
    /// </summary>
    /// <remarks>The Culture property is created from Code in the constructor. The constructor throws
    /// CultureNotFoundException if Code is not a valid culture name.</remarks>
    public class CultureInfoSettings
    {
        /// <summary>
        /// Gets or sets the code.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets the name that identifies the instance.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the CultureInfo used for culture-specific formatting, parsing, and comparisons.
        /// </summary>
        /// <remarks>Used by the type's members to perform culture-sensitive operations. Avoid changing
        /// this property concurrently from multiple threads.</remarks>
        public CultureInfo Culture { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        /// <param name="name"></param>
        public CultureInfoSettings(string code, string name)
        {
            Code = code;
            Name = name;
            Culture = new CultureInfo(code);
        }
    }
}
