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
using System.Text.RegularExpressions;
using VDS.RDF.Parsing.Tokens;

namespace VDS.RDF.Parsing
{
    /// <summary>
    /// Acceptable Turtle syntaxes
    /// </summary>
    public enum TurtleSyntax
    {
        /// <summary>
        /// Turtle as originally specified by the <a href="http://www.w3.org/TeamSubmission/turtle/">Turtle Submission</a>
        /// </summary>
        Original,
        /// <summary>
        /// Turtle as standardised by the <a href="http://www.w3.org/TR/turtle/">W3C RDF Working Group</a>
        /// </summary>
        W3C
    }

    /// <summary>
    /// Helper function relating to the Turtle Specifications
    /// </summary>
    /// <remarks>Not currently used in the actual <see cref="TurtleTokeniser">TurtleTokeniser</see> or <see cref="TurtleParser">TurtleParser</see> but is used for the new <see cref="TriGTokeniser">TriGTokeniser</see></remarks>
    public class TurtleSpecsHelper
    {
        /// <summary>
        /// Pattern for Valid Integers in Turtle
        /// </summary>
        public const String ValidIntegerPattern = "^(\\+|-)?\\d+$";
        /// <summary>
        /// Pattern for Valid Decimals in Turtle
        /// </summary>
        public const String ValidDecimalPattern = "^(\\+|-)?(\\d+\\.\\d*|\\.\\d+|\\d+)$";
        /// <summary>
        /// Pattern for Valid Doubles in Turtle
        /// </summary>
        public const String ValidDoublePattern = "^(\\+|-)?(\\d+\\.\\d+[eE](\\+|-)?\\d+|\\.\\d+[eE](\\+|-)?\\d+|\\d+[eE](\\+|-)?\\d+)$";
        /// <summary>
        /// Pattern for determining whether a given String should be serialized as a Long Literal
        /// </summary>
        public const String LongLiteralsPattern = "[\n\r\t\"]";

        private static Regex _validInteger = new Regex(ValidIntegerPattern);
        private static Regex _validDecimal = new Regex(ValidDecimalPattern);
        private static Regex _validDouble = new Regex(ValidDoublePattern);
        private static Regex _isLongLiteral = new Regex(LongLiteralsPattern);

        /// <summary>
        /// Determines whether a given String is a valid Plain Literal
        /// </summary>
        /// <param name="value">String to test</param>
        /// <returns></returns>
        public static bool IsValidPlainLiteral(String value, TurtleSyntax syntax)
        {
            StringComparison comparison = (syntax == TurtleSyntax.Original ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);
            if (value.Equals("true", comparison) || value.Equals("false", comparison))
            {
                return true;
            } 
            else 
            {
                return (_validDecimal.IsMatch(value) || _validInteger.IsMatch(value) || _validDouble.IsMatch(value));
            }
        }

        /// <summary>
        /// Determines whether a given String is a valid Plain Literal for the given Datatype
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="dt">Datatype</param>
        /// <returns></returns>
        public static bool IsValidPlainLiteral(String value, Uri dt, TurtleSyntax syntax)
        {
            StringComparison comparison = (syntax == TurtleSyntax.Original ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);
            if ((value.Equals("true", comparison) || value.Equals("false", comparison)) && dt.ToSafeString().Equals(XmlSpecsHelper.XmlSchemaDataTypeBoolean))
            {
                return true;
            }
            else if (_validDecimal.IsMatch(value) && dt.ToSafeString().Equals(XmlSpecsHelper.XmlSchemaDataTypeDecimal))
            {
                return true;
            }
            else if (_validInteger.IsMatch(value) && dt.ToSafeString().Equals(XmlSpecsHelper.XmlSchemaDataTypeInteger))
            {
                return true;
            }
            else if (_validDouble.IsMatch(value) && dt.ToSafeString().Equals(XmlSpecsHelper.XmlSchemaDataTypeDouble))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Determines whether a given String is a valid Integer
        /// </summary>
        /// <param name="value">String to test</param>
        /// <returns></returns>
        public static bool IsValidInteger(String value)
        {
            return _validInteger.IsMatch(value);
        }

        /// <summary>
        /// Determines whether a given String is a valid Decimal
        /// </summary>
        /// <param name="value">String to test</param>
        /// <returns></returns>
        public static bool IsValidDecimal(String value)
        {
            return _validDecimal.IsMatch(value);
        }

        /// <summary>
        /// Determines whether a given String is a valid Double
        /// </summary>
        /// <param name="value">String to test</param>
        /// <returns></returns>
        public static bool IsValidDouble(String value)
        {
            return _validDouble.IsMatch(value);
        }

        public static bool IsValidQName(String value)
        {
            return IsValidQName(value, TurtleSyntax.Original);
        }

        /// <summary>
        /// Determines whether a given String is a valid QName
        /// </summary>
        /// <param name="value">String to test</param>
        /// <returns></returns>
        public static bool IsValidQName(String value, TurtleSyntax syntax)
        {
            if (value.Contains(':'))
            {
                String ns, localname;
                ns = value.Substring(0, value.IndexOf(':'));
                localname = value.Substring(value.IndexOf(':') + 1);

                //Namespace Validation
                if (!ns.Equals(String.Empty))
                {
                    //Allowed empty Namespace
                    if (ns.StartsWith("-") || ns.StartsWith("."))
                    {
                        //Can't start with a - or .
                        return false;
                    }
                    else
                    {
                        char[] nchars = ns.ToCharArray();
                        if (IsPNCharsBase(nchars[0]))
                        {
                            if (nchars.Length > 1)
                            {
                                for (int i = 1; i < nchars.Length; i++)
                                {
                                    //Check if valid Name Char
                                    //The . character is not allowed in original Turtle but is permitted in new Turtle
                                    if (!IsPNChars(nchars[i]) || (nchars[i] == '.' && syntax == TurtleSyntax.Original)) return false;
                                }
                                //If we reach here the Namespace is OK
                            }
                            else
                            {
                                //Only 1 Character which was valid so OK
                            }
                        }
                        else
                        {
                            //Doesn't start with a valid Name Start Char
                            return false;
                        }
                    }
                }

                //Local Name Validation
                if (!localname.Equals(String.Empty))
                {
                    //Allowed empty Local Name
                    char[] lchars = localname.ToCharArray();

                    if (IsPNCharsU(lchars[0]) || (Char.IsDigit(lchars[0]) && syntax == TurtleSyntax.W3C))
                    {
                        if (lchars.Length > 1)
                        {
                            for (int i = 1; i < lchars.Length; i++)
                            {
                                //Check if valid Name Char
                                //The . character is not allowed in original Turtle but is permitted in new Turtle
                                if (!IsPNChars(lchars[i]) || (lchars[i] == '.' && syntax == TurtleSyntax.Original)) return false;
                            }
                            //If we reach here the Local Name is OK
                        }
                        else
                        {
                            //Only 1 Character which was valid so OK
                        }
                    }
                    else
                    {
                        //Not a valid Name Start Char
                        return false;
                    }
                }

                //If we reach here then it's all valid
                return true;
            }
            else
            {
                //Must contain a colon
                return false;
            }
        }

        /// <summary>
        /// Determines whether a given String should be serialized as a Long Literal
        /// </summary>
        /// <param name="value">String to test</param>
        /// <returns></returns>
        public static bool IsLongLiteral(String value)
        {
            return _isLongLiteral.IsMatch(value);
        }

        /// <summary>
        /// Infers the Type of a Plain Literal
        /// </summary>
        /// <param name="p">Plain Literal to infer the Type of</param>
        /// <returns>A Uri  representing the XML Scheme Data Type for the Plain Literal</returns>
        public static Uri InferPlainLiteralType(PlainLiteralToken p, TurtleSyntax syntax)
        {
            String value = p.Value;
            StringComparison comparison = (syntax == TurtleSyntax.Original ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);

            if (value.Equals("true", comparison) || value.Equals("false", comparison))
            {
                //Is a Boolean
                return UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeBoolean);
            }
            else if (_validInteger.IsMatch(value)) 
            {
                //Is an Integer
                return UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeInteger);
            }
            else if (_validDecimal.IsMatch(value))
            {
                //Is a Decimal
                return UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDecimal);
            }
            else if (_validDouble.IsMatch(value))
            {
                //Is a Double
                return UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDouble);
            }
            else
            {
                throw new RdfParseException("Unable to automatically Infer a Type for this PlainLiteralToken.  Plain Literals may only be Booleans, Integers, Decimals or Doubles");
            }
        }

        public static bool IsPNCharsBase(char c)
        {
            if (c >= 'a' && c <= 'z')
            {
                return true;
            }
            else if (c >= 'A' && c <= 'Z')
            {
                return true;
            }
            else if ((c >= 0x00c0 && c <= 0x00d6) ||
                     (c >= 0x00d8 && c <= 0x00f6) ||
                     (c >= 0x00f8 && c <= 0x02ff) ||
                     (c >= 0x0370 && c <= 0x037d) ||
                     (c >= 0x200c && c <= 0x200d) ||
                     (c >= 0x2070 && c <= 0x218f) ||
                     (c >= 0x2c00 && c <= 0x2fef) ||
                     (c >= 0x3001 && c <= 0xd7ff) ||
                     (c >= 0xf900 && c <= 0xfdcf) ||
                     (c >= 0xfdf0 && c <= 0xfffd) /*||
                     (c >= 0x10000 && c <= 0xeffff)*/)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool IsPNChars(char c)
        {
            if (IsPNCharsU(c))
            {
                return true;
            }
            else if (c == '-')
            {
                return true;
            }
            else if (Char.IsDigit(c))
            {
                return true;
            }
            else if (c == 0x00b7)
            {
                return true;
            }
            else if (c >= 0x0300 && c <= 0x036f)
            {
                return true;
            }
            else if (c >= 0x203f && c <= 0x2040)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool IsPNCharsU(char c)
        {
            return c == '_' || IsPNCharsBase(c);
        }
    }

}
