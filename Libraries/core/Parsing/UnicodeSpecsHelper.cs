/*

Copyright Robert Vesse 2009-10
rvesse@vdesign-studios.com

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

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
            switch (Char.GetUnicodeCategory(c))
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
            switch (Char.GetUnicodeCategory(c))
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
            switch (Char.GetUnicodeCategory(c))
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
