/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace VDS.RDF.Parsing
{
    /// <summary>
    /// Helper Class which defines some Test Functions for testing the Unicode Category of Characters
    /// </summary>
    public class UnicodeSpecsHelper
    {
        /// <summary>
        /// Checks whether a given Character is considered a Letter
        /// </summary>
        /// <param name="c">Character to Test</param>
        /// <returns></returns>
        public static bool IsLetter(char c)
        {
            switch (GetUnicodeCategory(c))
            {
                case UnicodeCategory.LowercaseLetter:
                case UnicodeCategory.ModifierLetter:
                case UnicodeCategory.OtherLetter:
                case UnicodeCategory.TitlecaseLetter:
                case UnicodeCategory.UppercaseLetter:
                    return true;
                default:
                    return false;
            }
        }

        private static UnicodeCategory GetUnicodeCategory(char c)
        {
#if PORTABLE
            return CharUnicodeInfo.GetUnicodeCategory(c);
#else
            return Char.GetUnicodeCategory(c);
#endif
        }
        /// <summary>
        /// Checks whether a given Character is considered a Letter or Digit
        /// </summary>
        /// <param name="c">Character to Test</param>
        /// <returns></returns>
        public static bool IsLetterOrDigit(char c)
        {
            return (UnicodeSpecsHelper.IsLetter(c) || UnicodeSpecsHelper.IsDigit(c));
        }

        /// <summary>
        /// Checks whether a given Character is considered a Letter Modifier
        /// </summary>
        /// <param name="c">Character to Test</param>
        /// <returns></returns>
        public static bool IsLetterModifier(char c)
        {
            switch (GetUnicodeCategory(c))
            {
                case UnicodeCategory.ModifierLetter:
                case UnicodeCategory.NonSpacingMark:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Checks whether a given Character is considered a Digit
        /// </summary>
        /// <param name="c">Character to Test</param>
        /// <returns></returns>
        public static bool IsDigit(char c)
        {
            switch (GetUnicodeCategory(c))
            {
                case UnicodeCategory.DecimalDigitNumber:
                case UnicodeCategory.LetterNumber:
                case UnicodeCategory.OtherNumber:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Converts a Hex Escape into the relevant Unicode Character
        /// </summary>
        /// <param name="hex">Hex code</param>
        /// <returns></returns>
        public static char ConvertToChar(String hex)
        {
            try
            {
                //Convert to an Integer
                int i = Convert.ToInt32(hex, 16);
                //Try to cast to a Char
                char c = (char)i;
                //Append to Output
                return c;
            }
            catch
            {
                throw new RdfParseException("Unable to convert the String '" + hex + "' into a Unicode Character");
            }
        }
    }
}
