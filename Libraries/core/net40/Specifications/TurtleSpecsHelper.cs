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
using System.Text.RegularExpressions;
using VDS.RDF.Parsing.Tokens;

namespace VDS.RDF.Specifications
{
    /// <summary>
    /// Acceptable Turtle syntaxes
    /// </summary>
    public enum TurtleSyntax
    {
        /// <summary>
        /// Turtle as originally specified by the <a href="http://www.w3.org/TeamSubmission/turtle/">Turtle Team Submission</a>
        /// </summary>
        Original,
        /// <summary>
        /// Turtle as standardised by the <a href="http://www.w3.org/TR/turtle/">W3C RDF Working Group</a>
        /// </summary>
        W3C
    }

    /// <summary>
    /// Acceptable TriG syntaxes
    /// </summary>
    public enum TriGSyntax
    {
        /// <summary>
        /// TriG as originally <a href="http://www4.wiwiss.fu-berlin.de/bizer/trig/">specified</a>.  @base is not permitted and @prefix may only occur outside of graphs
        /// </summary>
        Original,
        /// <summary>
        /// TriG as specified by the <a href="http://www.w3.org/2010/01/Turtle/Trig">TriG Member Submission</a>.  @base is permitted and both @base and @prefix may occur both inside and outside graphs but the tokens use Turtle Team Submission rules i.e. newer escape sequences and other changes in the official W3C specification of Turtle do not apply.
        /// </summary>
        MemberSubmission
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
        /// <param name="syntax">Turtle Syntax</param>
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
        /// <param name="syntax">Turtle Syntax</param>
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

        /// <summary>
        /// Gets whether a QName is valid in Turtle (assumes Turtle as originally specified by Dave Beckett)
        /// </summary>
        /// <param name="value">QName</param>
        /// <returns></returns>
        public static bool IsValidQName(String value)
        {
            return IsValidQName(value, TurtleSyntax.Original);
        }

        /// <summary>
        /// Determines whether a given String is a valid QName
        /// </summary>
        /// <param name="value">String to test</param>
        /// <param name="syntax">Turtle Syntax</param>
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
        /// Gets whether a character matches the PN_CHARS_BASE production from the Turtle specifications
        /// </summary>
        /// <param name="c">Character</param>
        /// <returns></returns>
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

        /// <summary>
        /// Gets whether a character matches the PN_CHARS production from the Turtle specification
        /// </summary>
        /// <param name="c">Character</param>
        /// <returns></returns>
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

        /// <summary>
        /// Gets whether a character matches the PN_CHARS_U production from the Turtle specification
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsPNCharsU(char c)
        {
            return c == '_' || IsPNCharsBase(c);
        }
    }

}
