// "Therefore those skilled at the unorthodox
// are infinite as heaven and earth,
// inexhaustible as the great rivers.
// When they come to an end,
// they bagin again,
// like the days and months;
// they die and are reborn,
// like the four seasons."
// 
// - Sun Tsu,
// "The Art of War"

using System;
using System.Collections.Generic;
using System.Drawing;

namespace HtmlRenderer.Utils
{
    /// <summary>
    /// Utils for fonts and fonts families handling.
    /// </summary>
    internal static class FontsUtils
    {
        #region Fields and Consts

        /// <summary>
        /// Allow to map not installed fonts to different
        /// </summary>
        private static readonly Dictionary<string, string> _fontsMapping = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// collection of all installed and added font families to check if font exists
        /// </summary>
        private static readonly Dictionary<string, FontFamily> _existingFontFamilies = new Dictionary<string, FontFamily>(StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// cache of all the font used not to create same font again and again
        /// </summary>
        private static readonly Dictionary<string, Dictionary<float, Dictionary<FontStyle, Font>>> _fontsCache = new Dictionary<string, Dictionary<float, Dictionary<FontStyle, Font>>>(StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// cache of the font sizes not to call Font.GetHeight() each time it is required
        /// </summary>
        private static readonly Dictionary<Font, float> _fontHeightCache = new Dictionary<Font, float>();

        #endregion


        /// <summary>
        /// Init the system installed fonts.
        /// </summary>
        static FontsUtils()
        {
            _fontsMapping["monospace"] = "Courier New";
            _fontsMapping["Helvetica"] = "Arial";

            foreach (var family in FontFamily.Families)
            {
                _existingFontFamilies.Add(family.Name, family);
            }
        }

        /// <summary>
        /// Check if the given font family exists by name
        /// </summary>
        /// <param name="family">the font to check</param>
        /// <returns>true - font exists by given family name, false - otherwise</returns>
        public static bool IsFontExists(string family)
        {
            bool exists = _existingFontFamilies.ContainsKey(family);
            if(!exists)
            {
                string mappedFamily;
                if(_fontsMapping.TryGetValue(family,out mappedFamily))
                {
                    exists = _existingFontFamilies.ContainsKey(mappedFamily);                    
                }
            }
            return exists;
        }

        /// <summary>
        /// Get cached font instance for the given font properties.<br/>
        /// Improve performance not to create same font multiple times.
        /// </summary>
        /// <returns>cached font instance</returns>
        public static Font GetCachedFont(string family, float size, FontStyle style)
        {
            var font = TryGetFont(family, size, style);
            if (font == null)
            {
                if (!_existingFontFamilies.ContainsKey(family))
                {
                    string mappedFamily;
                    if(_fontsMapping.TryGetValue(family, out mappedFamily))
                    {
                        font = TryGetFont(mappedFamily, size, style);
                        if(font == null)
                        {
                            font = CreateFont(mappedFamily, size, style);
                            _fontsCache[mappedFamily][size][style] = font;
                        }
                    }
                }

                if(font == null)
                {
                    font = CreateFont(family, size, style);
                }

                _fontsCache[family][size][style] = font;
            }
            return font;
        }

        /// <summary>
        /// Get cached font height for the given font.<br/>
        /// Improve performance not to access the GetHeight property of a font as it is expensive.<br/>
        /// Should be used with <see cref="GetCachedFont"/> as the cache uses the font object itself as key.
        /// </summary>
        /// <param name="font">the font to get its height</param>
        /// <returns>the height of the font</returns>
        public static float GetFontHeight(Font font)
        {
            float height;
            if (!_fontHeightCache.TryGetValue(font, out height))
            {
                _fontHeightCache[font] = height = font.GetHeight();
            }
            return height;
        }

        /// <summary>
        /// Adds a font family to be used.
        /// </summary>
        /// <param name="fontFamily">The font family to add.</param>
        public static void AddFontFamily(FontFamily fontFamily)
        {
            ArgChecker.AssertArgNotNull(fontFamily, "family");

            _existingFontFamilies[fontFamily.Name] = fontFamily;
        }

        /// <summary>
        /// Adds a font mapping from <paramref name="fromFamily"/> to <paramref name="toFamily"/> iff the <paramref name="fromFamily"/> is not found.<br/>
        /// When the <paramref name="fromFamily"/> font is used in rendered html and is not found in existing 
        /// fonts (installed or added) it will be replaced by <paramref name="toFamily"/>.<br/>
        /// </summary>
        /// <param name="fromFamily">the font family to replace</param>
        /// <param name="toFamily">the font family to replace with</param>
        public static void AddFontFamilyMapping(string fromFamily, string toFamily)
        {
            ArgChecker.AssertArgNotNullOrEmpty(fromFamily, "fromFamily");
            ArgChecker.AssertArgNotNullOrEmpty(toFamily, "toFamily");

            _fontsMapping[fromFamily] = toFamily;
        }


        #region Private methods

        /// <summary>
        /// Get cached font if it exists in cache or null if it is not.
        /// </summary>
        private static Font TryGetFont(string family, float size, FontStyle style)
        {
            Font font = null;
            if (_fontsCache.ContainsKey(family))
            {
                var a = _fontsCache[family];
                if (a.ContainsKey(size))
                {
                    var b = a[size];
                    if (b.ContainsKey(style))
                    {
                        font = b[style];
                    }
                }
                else
                {
                    _fontsCache[family][size] = new Dictionary<FontStyle, Font>();
                }
            }
            else
            {
                _fontsCache[family] = new Dictionary<float, Dictionary<FontStyle, Font>>();
                _fontsCache[family][size] = new Dictionary<FontStyle, Font>();
            }
            return font;
        }

        /// <summary>
        // create font (try using existing font family to support custom fonts)
        /// </summary>
        private static Font CreateFont(string family, float size, FontStyle style)
        {
            FontFamily fontFamily;
            return _existingFontFamilies.TryGetValue(family, out fontFamily) ? new Font(fontFamily, size, style) : new Font(family, size, style);
        }

        #endregion
    }
}