/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/
#if !NETSTANDARD2_0
using System;
using System.Text;
using VDS.RDF.Parsing;

namespace System.Web
{
    /// <summary>
    /// An implementation of HttpUtility for use with Silverlight builds which require it
    /// </summary>
    /// <remarks>
    /// <para>
    /// The URL Encoding algorithm is partially based on an algorithm presented in this <a href="http://www.codeguru.com/cpp/cpp/cpp_mfc/article.php/c4029">CodeGuru</a> article
    /// </para>
    /// </remarks>
    public static class HttpUtility
    {
        /// <summary>
        /// HTML Decodes a String so any character entities used are converted to their actual characters
        /// </summary>
        /// <param name="value">Value to decode</param>
        /// <returns></returns>
        public static String HtmlDecode(String value)
        {
            return System.Net.WebUtility.HtmlDecode(value);
        }

        /// <summary>
        /// HTML Encodes a String so any that requires entitzing are converted to character entities
        /// </summary>
        /// <param name="value">Value to encode</param>
        /// <returns></returns>
        public static String HtmlEncode(String value)
        {
            return System.Net.WebUtility.HtmlEncode(value);
        }

        /// <summary>
        /// Encodes a URL string so any characters that require percent encoding are encoded
        /// </summary>
        /// <param name="value">Value to encode</param>
        /// <returns></returns>
        public static String UrlEncode(String value)
        {
            if (!IsUnsafeUrlString(value))
            {
                return value;
            }
            else
            {
                char c, d, e;
                StringBuilder output = new StringBuilder();
                for (int i = 0; i < value.Length; i++)
                {
                    c = value[i];
                    if (!IsSafeCharacter(c))
                    {
                        if (c == '%')
                        {
                            if (i <= value.Length - 2)
                            {
                                d = value[i + 1];
                                e = value[i + 2];
                                if (IriSpecsHelper.IsHexDigit(d) && IriSpecsHelper.IsHexDigit(e))
                                {
                                    // Has valid hex digits after it so continue encoding normally
                                    output.Append(c);
                                }
                                else
                                {
                                    // Need to encode a bare percent character
                                    output.Append(PercentEncode(c));
                                }
                            }
                            else
                            {
                                // Not enough characters after a % to use as a valid escape so encode the percent
                                output.Append(PercentEncode(c));
                            }
                        }
                        else
                        {
                            // Contains an unsafe character so percent encode
                            output.Append(PercentEncode(c));
                        }
                    }
                    else
                    {
                        // No need to encode safe characters
                        output.Append(c);
                    }
                }
                return output.ToString();
            }
        }

        /// <summary>
        /// Decodes a URL string so any characters that are percent encoded are converted to actual characters
        /// </summary>
        /// <param name="value">Value to decode</param>
        /// <returns></returns>
        public static String UrlDecode(String value)
        {
            // Safe to use this regardless of String length as no limit on input size
            return Uri.UnescapeDataString(value);
        }

        private static bool IsUnsafeUrlString(String value)
        {
            char c, d, e;
            for (int i = 0; i < value.Length; i++)
            {
                c = value[i];
                if (!IsSafeCharacter(c))
                {
                    if (c == '%')
                    {
                        if (i <= value.Length - 2)
                        {
                            d = value[i + 1];
                            e = value[i + 2];
                            if (IriSpecsHelper.IsHexDigit(d) && IriSpecsHelper.IsHexDigit(e))
                            {
                                i += 2;
                                continue;
                            }
                            else
                            {
                                // Expected two hex digits after a % as an escape
                                return true;
                            }
                        }
                        else
                        {
                            // Not enough characters after a % to use as a valid escape
                            return true;
                        }
                    }
                    else
                    {
                        // Contains an unsafe character
                        return true;
                    }
                }
            }

            // All Characters OK
            return false;
        }

        private static bool IsSafeCharacter(char c)
        {
            // Safe characters which should not be percent encoded per RFC 3986 Section 2.3
            // http://tools.ietf.org/html/rfc3986#section-2.3
            // Alpha (65-90 and 97-122), Digits (48-57), Hyphen (45), Period (46), Underscore (95) and Tilde (126)
            if ((c >= 65 && c <= 90) || (c >= 97 && c <= 122) || (c >= 48 && c <= 57) || c == 45 || c == 46 || c == 95 || c == 126)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static String PercentEncode(char c)
        {
            if (c <= 255)
            {
                // Can be encoded in a single percent encode
                if (c <= 127)
                {
                    return "%" + ((int)c).ToString("X2");
                }
                else
                {
                    byte[] codepoints = Encoding.UTF8.GetBytes(new char[] { c });
                    StringBuilder output = new StringBuilder();
                    foreach (byte b in codepoints)
                    {
                        output.Append("%");
                        output.Append(((int)b).ToString("X2"));
                    }
                    return output.ToString();
                }
            }
            else
            {
                // Unicode character so requires more than one percent encode
                byte[] codepoints = Encoding.UTF8.GetBytes(new char[] { c });
                StringBuilder output = new StringBuilder();
                foreach (byte b in codepoints)
                {
                    output.Append("%");
                    output.Append(((int)b).ToString("X2"));
                }
                return output.ToString();
            }
        }
    }
}
#endif