using System;

namespace VDS.RDF.Compatability
{
    /// <summary>
    /// Helper class, which makes up for the lack of some <see cref="char"/> methods in Silverlight
    /// </summary>
    public static class CharHelper
    {
        /// <summary>
        /// Indicates whether the specified <see cref="T:System.Char"/> object is a high surrogate.
        /// </summary>
        /// 
        /// <returns>
        /// true if the numeric value of the <paramref name="c"/> parameter ranges from U+D800 through U+DBFF; otherwise, false.
        /// </returns>
        /// <param name="c">The Unicode character to evaluate. </param><filterpriority>1</filterpriority>
        public static bool IsHighSurrogate(char c)
        {
#if SILVERLIGHT
            if (c >= 55296)
                return c <= 56319;

            return false;
#else
             return char.IsHighSurrogate(c);
#endif
        }

        /// <summary>
        /// Indicates whether the specified <see cref="T:System.Char"/> object is a low surrogate.
        /// </summary>
        /// 
        /// <returns>
        /// true if the numeric value of the <paramref name="c"/> parameter ranges from U+DC00 through U+DFFF; otherwise, false.
        /// </returns>
        /// <param name="c">The character to evaluate. </param><filterpriority>1</filterpriority>
        public static bool IsLowSurrogate(char c)
        {
#if SILVERLIGHT
            if (c >= 56320)
                return c <= 57343;

            return false;
#else
            return char.IsLowSurrogate(c);
#endif
        }

        /// <summary>
        /// Converts the value of a UTF-16 encoded surrogate pair into a Unicode code point.
        /// </summary>
        /// 
        /// <returns>
        /// The 21-bit Unicode code point represented by the <paramref name="highSurrogate"/> and <paramref name="lowSurrogate"/> parameters.
        /// </returns>
        /// <param name="highSurrogate">A high surrogate code point (that is, a code point ranging from U+D800 through U+DBFF). </param><param name="lowSurrogate">A low surrogate code point (that is, a code point ranging from U+DC00 through U+DFFF). </param><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="highSurrogate"/> is not in the range U+D800 through U+DBFF, or <paramref name="lowSurrogate"/> is not in the range U+DC00 through U+DFFF. </exception><filterpriority>1</filterpriority>
        public static int ConvertToUtf32(char highSurrogate, char lowSurrogate)
        {
#if SILVERLIGHT
            if (!IsHighSurrogate(highSurrogate))
                throw new ArgumentOutOfRangeException("highSurrogate");
            if (!IsLowSurrogate(lowSurrogate))
                throw new ArgumentOutOfRangeException("lowSurrogate");
            
            return (highSurrogate - 55296) * 1024 + (lowSurrogate - 56320) + 65536;
#else
            return char.ConvertToUtf32(highSurrogate, lowSurrogate);
#endif
        }
    }
}