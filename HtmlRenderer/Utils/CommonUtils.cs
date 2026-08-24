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
using System.IO;
using System.Net;
using System.Text;

namespace HtmlRenderer.Utils
{
    public delegate void Action<in T>(T obj);

    public delegate void Action<in T1, in T2>(T1 arg1, T2 arg2);

    public delegate void Action<in T1, in T2, in T3>(T1 arg1, T2 arg2, T3 arg3);

    /// <summary>
    /// Utility methods for general stuff.
    /// </summary>
    internal static class CommonUtils
    {
        /// <summary>
        /// Get size that is max of <paramref name="size"/> and <paramref name="other"/> for width and height seperatly.
        /// </summary>
        public static SizeF Max(SizeF size, SizeF other)
        {
            return new SizeF(Math.Max(size.Width, other.Width), Math.Max(size.Height, other.Height));
        }

        /// <summary>
        /// Get Uri object for the given path if it is valid uri path.
        /// </summary>
        /// <param name="path">the path to get uri for</param>
        /// <returns>uri or null if not valid</returns>
        public static Uri TryGetUri(string path)
        {
            try
            {
                if (Uri.IsWellFormedUriString(path, UriKind.RelativeOrAbsolute))
                {
                    return new Uri(path);
                }
            }
            catch
            {}

            return null;
        }

        /// <summary>
        /// Get the first value in the given dictionary.
        /// </summary>
        /// <typeparam name="TKey">the type of dictionary key</typeparam>
        /// <typeparam name="TValue">the type of dictionary value</typeparam>
        /// <param name="dic">the dictionary</param>
        /// <param name="defaultValue">optional: the default value to return of no elements found in dictionary </param>
        /// <returns>first element or default value</returns>
        public static TValue GetFirstValueOrDefault<TKey, TValue>(IDictionary<TKey, TValue> dic, TValue defaultValue = default(TValue))
        {
            if(dic != null)
            {
                foreach (var value in dic)
                    return value.Value;
            }
            return defaultValue;
        }

        /// <summary>
        /// Get file info object for the given path if it is valid file path.
        /// </summary>
        /// <param name="path">the path to get file info for</param>
        /// <returns>file info or null if not valid</returns>
        public static FileInfo TryGetFileInfo(string path)
        {
            try
            {
                return new FileInfo(path);
            }
            catch
            { }

            return null;
        }

        /// <summary>
        /// Get web client response content type.
        /// </summary>
        /// <param name="client">the web client to get the response content type from</param>
        /// <returns>response content type or null</returns>
        public static string GetResponseContentType(WebClient client)
        {
            foreach (string header in client.ResponseHeaders)
            {
                if (header.Equals("Content-Type", StringComparison.InvariantCultureIgnoreCase))
                    return client.ResponseHeaders[header];

            }
            return null;
        }

        /// <summary>
        /// Gets the representation of the online uri on the local disk.
        /// </summary>
        /// <param name="imageUri">The online image uri.</param>
        /// <returns>The path of the file on the disk.</returns>
        public static FileInfo GetLocalfileName(Uri imageUri)
        {
            StringBuilder fileNameBuilder = new StringBuilder();
            string absoluteUri = imageUri.AbsoluteUri;
            int lastSlash = absoluteUri.LastIndexOf('/');
            if (lastSlash == -1)
            {
                return null;
            }

            string uriUntilSlash = absoluteUri.Substring(0, lastSlash);
            fileNameBuilder.Append(uriUntilSlash.GetHashCode().ToString());
            fileNameBuilder.Append('_');

            string restOfUri = absoluteUri.Substring(lastSlash + 1);
            int indexOfParams = restOfUri.IndexOf('?');
            if (indexOfParams == -1)
            {
                string ext = ".cache";
                int indexOfDot = restOfUri.IndexOf('.');
                if (indexOfDot > -1)
                {
                    ext = restOfUri.Substring(indexOfDot);
                    restOfUri = restOfUri.Substring(0, indexOfDot);
                }

                fileNameBuilder.Append(restOfUri);
                fileNameBuilder.Append(ext);
            }
            else
            {
                int indexOfDot = restOfUri.IndexOf('.');
                if (indexOfDot == -1 || indexOfDot > indexOfParams)
                {
                    //The uri is not for a filename
                    fileNameBuilder.Append(restOfUri);
                    fileNameBuilder.Append(".cache");
                }
                else if (indexOfParams > indexOfDot)
                {
                    //Adds the filename without extension.
                    fileNameBuilder.Append(restOfUri, 0, indexOfDot);
                    //Adds the parameters
                    fileNameBuilder.Append(restOfUri, indexOfParams, restOfUri.Length - indexOfParams);
                    //Adds the filename extension.
                    fileNameBuilder.Append(restOfUri, indexOfDot, indexOfParams - indexOfDot);
                }
            }

            var validFileName = GetValidFileName(fileNameBuilder.ToString());
            if (validFileName.Length > 25)
            {
                validFileName = validFileName.Substring(0, 24) + validFileName.Substring(24).GetHashCode() + Path.GetExtension(validFileName);
            }

            return new FileInfo(Path.Combine(Path.GetTempPath(), validFileName));
        }

        /// <summary>
        /// Get substring seperated by whitespace starting from the given idex.
        /// </summary>
        /// <param name="str">the string to get substring in</param>
        /// <param name="idx">the index to start substring search from</param>
        /// <param name="length">return the length of the found string</param>
        /// <returns>the index of the substring, -1 if no valid sub-string found</returns>
        public static int GetNextSubString(string str, int idx, out int length)
        {
            while (idx < str.Length && Char.IsWhiteSpace(str[idx]))
                idx++;
            if (idx < str.Length)
            {
                var endIdx = idx + 1;
                while (endIdx < str.Length && !Char.IsWhiteSpace(str[endIdx]))
                    endIdx++;
                length = endIdx - idx;
                return idx;
            }
            length = 0;
            return -1;
        }
        
        /// <summary>
        /// Compare that the substring of <paramref name="str"/> is equal to <paramref name="str"/>
        /// Assume given substring is not empty and all indexes are valid!<br/>
        /// </summary>
        /// <returns>true - equals, false - not equals</returns>
        public static bool SubStringEquals(string str, int idx, int length, string str2)
        {
            if (length == str2.Length && idx + length <= str.Length)
            {
                for (int i = 0; i < length; i++)
                {
                    if (Char.ToLowerInvariant(str[idx + i]) != Char.ToLowerInvariant(str2[i]))
                        return false;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Replaces invalid filename chars to '_'
        /// </summary>
        /// <param name="source">The possibly-not-valid filename</param>
        /// <returns>A valid filename.</returns>
        private static string GetValidFileName(string source)
        {
            string retVal = source;
            char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
            foreach (var invalidFileNameChar in invalidFileNameChars)
            {
                retVal = retVal.Replace(invalidFileNameChar, '_');
            }
            return retVal;
        }
    }
}
