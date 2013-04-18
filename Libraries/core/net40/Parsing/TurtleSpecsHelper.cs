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
using VDS.RDF.Query;

namespace VDS.RDF.Parsing
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

        public static bool IsValidPrefix(String value, TurtleSyntax syntax)
        {
            //W3C Standard Turtle
            //PNAME_NS	::=	PN_PREFIX? ':'

            //Original Member Submission Turtle
            //qname	::=	prefixName? ':' name?

            //The productions are identical for our purposes
            if (value.Equals(String.Empty)) return false;
            if (!value.EndsWith(":")) return false;
            if (value.Equals(":")) return true;

            //IsPNPrefix() implements the appropriate productions for the different syntaxes
            return IsPNPrefix(value.Substring(0, value.Length - 1), syntax);
        }

        public static bool IsPNPrefix(String value, TurtleSyntax syntax)
        {
            char[] cs = value.ToCharArray();
            int start = 1;
            switch (syntax)
            {
                case TurtleSyntax.W3C:
                    //PN_PREFIX	::=	PN_CHARS_BASE ((PN_CHARS | '.')* PN_CHARS)?

                    if (cs.Length == 0) return true;

                    //First character must be in PN_CHARS_BASE
                    if (!IsPNCharsBase(cs[0])) 
                    {
                        //Handle surrogate pairs for UTF-32 characters
                        if (Char.IsHighSurrogate(cs[0]) && cs.Length > 1)
                        {
                            if (!IsPNCharsBase(cs[0], cs[1])) return false;
                            start++;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    if (cs.Length == start) return true;

                    //Intermediate characters must be a '.' or in PN_CHARS
                    for (int i = start; i < cs.Length - 1; i++)
                    {
                        if (cs[i] != '.' && !IsPNChars(cs[i]))
                        {
                            //Handle surrogate pairs for UTF-32 characters
                            if (Char.IsHighSurrogate(cs[i]) && i < cs.Length - 2)
                            {
                                if (!IsPNChars(cs[i], cs[i + 1])) return false;
                                i++;
                            }
                            else if (Char.IsHighSurrogate(cs[i]) && i == cs.Length - 2)
                            {
                                //This case handles the case where the final character is a UTF-32 character representing by a surrogate pair
                                return IsPNChars(cs[i], cs[i + 1]);
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }

                    //Final character must be in PN_CHARS
                    return IsPNChars(cs[cs.Length - 1]);

                default:
                    //prefixName	::=	( nameStartChar - '_' ) nameChar*

                    if (cs.Length == 0) return true;

                    //First character must be a name start char and not a _
                    if (!IsNameStartChar(cs[0]))
                    {
                        //Handle surrogate pairs for UTF-32
                        if (Char.IsHighSurrogate(cs[0]) && cs.Length > 1)
                        {
                            if (!IsNameStartChar(cs[0], cs[1])) return false;
                            start++;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    if (cs.Length == start) return true;

                    //Subsequent characters must be in nameChar
                    for (int i = start; i < cs.Length; i++)
                    {
                        if (!IsNameChar(cs[i]))
                        {
                            //Handle surrogate pairs for UTF-32
                            if (Char.IsHighSurrogate(cs[i]) && i < cs.Length - 1)
                            {
                                if (!IsNameChar(cs[i], cs[i + 1])) return false;
                                i++;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                    return true;
            }
        }

        public static bool IsValidLocalName(String value, TurtleSyntax syntax)
        {
            char[] cs = value.ToCharArray();

            //Empty local names are valid
            if (cs.Length == 0) return true;

            switch (syntax)
            {
                case TurtleSyntax.W3C:
                    //PNAME_LN	::=	PNAME_NS PN_LOCAL
                    //PNAME_NS	::=	PN_PREFIX? ':'

                    //Local name is a syntax of namespace segments
                    String[] portions = value.Split(':');

                    //Each non-final portion conforms to the PNAME_NS production
                    //This is a PN_PREFIX followed by a ':' so we can call IsPNPrefix() directly
                    //However we have to be careful because the final portion can contain bare : which we already split on
                    int p;
                    for (p = 0; p < portions.Length - 1; p++)
                    {
                        if (portions[p].Contains("%") || portions[p].Contains("\\")) break;
                        if (!IsPNPrefix(portions[p], syntax)) return false;
                    }

                    String final = portions[portions.Length - 1];
                    if (p < portions.Length - 1)
                    {
                        final = String.Join(":", portions, p, portions.Length - p);
                    }

                    //Final portion conforms to PN_LOCAL
                    return IsPNLocal(final);

                default:
                    //name	::=	nameStartChar nameChar*

                    int start = 1;

                    //Validate first character is a nameStartChar
                    if (!IsNameStartChar(cs[0]))
                    {
                        if (Char.IsHighSurrogate(cs[0]) && cs.Length > 1)
                        {
                            if (!IsNameStartChar(cs[0], cs[1])) return false;
                            start++;
                        }
                        else
                        {
                            return false;
                        }
                    }

                    if (cs.Length == start) return true;

                    //Further characters must be nameChar
                    for (int i = start; i < cs.Length; i++)
                    {
                        if (!IsNameChar(cs[i]))
                        {
                            if (Char.IsHighSurrogate(cs[i]) && i < cs.Length - 1)
                            {
                                if (!IsNameChar(cs[i], cs[i + 1])) return false;
                                i++;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                    return true;
            }
        }

        public static bool IsPNLocal(String value)
        {
            //PN_LOCAL	::=	(PN_CHARS_U | ':' | [0-9] | PLX) ((PN_CHARS | '.' | ':' | PLX)* (PN_CHARS | ':' | PLX))?

            char[] cs = value.ToCharArray();
            int start = 1;

            //Validate first character
            if (cs[0] != ':' && !Char.IsDigit(cs[0]) && !IsPLX(cs, 0, out start) && !IsPNCharsU(cs[0]))
            {
                //Handle surrogate pairs for UTF-32 characters
                if (Char.IsHighSurrogate(cs[0]) && cs.Length > 1)
                {
                    if (!IsPnCharsU(cs[0], cs[1])) return false;
                    start++;
                }
                else
                {
                    return false;
                }
            }

            if (start >= cs.Length) return true;

            //Intermediate characters can be PN_CHARS, a '.', a ':' or a PLX
            for (int i = start; i < cs.Length - 1; i++)
            {
                int j = i;
                if (cs[i] != '.' && cs[i] != ':' && !IsPNChars(cs[i]) && !IsPLX(cs, i, out j))
                {
                    //Handle surrogate pairs for UTF-32 characters
                    if (Char.IsHighSurrogate(cs[i]) && i <= cs.Length - 2)
                    {
                        if (!IsPNChars(cs[i], cs[i + 1])) return false;
                        i++;
                    }
                    else if (Char.IsHighSurrogate(cs[i]) && i == cs.Length - 2)
                    {
                        //This case handles the case where the final character is a UTF-32 character representing by a surrogate pair
                        return IsPNChars(cs[i], cs[i + 1]);
                    }
                }
                if (i != j)
                {
                    //This means we just saw a PLX
                    //Last thing being a PLX is valid
                    if (j == cs.Length - 1) return true;
                    //Otherwise adjust the index appropriately and continue checking further characters
                    i = j;
                }
            }

            //Final character is a ':' or a PN_CHARS
            return cs[cs.Length - 1] == ':' || IsPNChars(cs[cs.Length - 1]);
        }

        /// <summary>
        /// Checks whether a given String matches the PLX rule from the Turtle W3C Specification
        /// </summary>
        /// <param name="cs">String as character array</param>
        /// <param name="startIndex">Start Index</param>
        /// <param name="endIndex">Resulting End Index</param>
        /// <returns></returns>
        public static bool IsPLX(char[] cs, int startIndex, out int endIndex)
        {
            endIndex = startIndex;
            if (cs[startIndex] == '%')
            {
                if (startIndex > cs.Length - 2)
                {
                    //If we saw a base % but there are not two subsequent characters not a valid PLX escape
                    return false;
                }
                else
                {
                    char a = cs[startIndex + 1];
                    char b = cs[startIndex + 2];
                    if (IsHex(a) && IsHex(b))
                    {
                        //Valid % encoding
                        endIndex = startIndex + 2;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else if (cs[startIndex] == '\\')
            {
                if (startIndex >= cs.Length - 1)
                {
                    //If we saw a backslash but no subsequent character not a valid PLX escape
                    return false;
                }
                else
                {
                    char c = cs[startIndex + 1];
                    switch (c)
                    {
                        case '_':
                        case '~':
                        case '-':
                        case '.':
                        case '!':
                        case '$':
                        case '&':
                        case '\'':
                        case '(':
                        case ')':
                        case '*':
                        case '+':
                        case ',':
                        case ';':
                        case '=':
                        case '/':
                        case '?':
                        case '#':
                        case '@':
                        case '%':
                            //Valid Escape
                            endIndex = startIndex + 1;
                            return true;
                        default:
                            return false;
                    }
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gets whether a character is a Hex character
        /// </summary>
        /// <param name="c">Character</param>
        /// <returns></returns>
        public static bool IsHex(char c)
        {
            if (Char.IsDigit(c))
            {
                return true;
            }
            else
            {
                switch (c)
                {
                    case 'A':
                    case 'a':
                    case 'B':
                    case 'b':
                    case 'C':
                    case 'c':
                    case 'D':
                    case 'd':
                    case 'E':
                    case 'f':
                    case 'F':
                        return true;
                    default:
                        return false;
                }
            }
        }

        /// <summary>
        /// Determines whether a given String is a valid QName
        /// </summary>
        /// <param name="value">String to test</param>
        /// <param name="syntax">Turtle Syntax</param>
        /// <returns></returns>
        public static bool IsValidQName(String value, TurtleSyntax syntax)
        {
            if (!value.Contains(":")) return false;
            String prefix = value.Substring(0, value.IndexOf(':') + 1);
            String lname = prefix.Length < value.Length ? value.Substring(prefix.Length) : String.Empty;
            return IsValidPrefix(prefix, syntax) && IsValidLocalName(lname, syntax);
        }

        /// <summary>
        /// Unescapes local name escapes in a QName
        /// </summary>
        /// <param name="value">QName</param>
        /// <returns>Unescaped QName</returns>
        public static String UnescapeQName(String value)
        {
            return SparqlSpecsHelper.UnescapeQName(value);
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
        /// <param name="syntax">Turtle Syntax</param>
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

        #region W3C Standardised Turtle Character Productions

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
                     (c >= 0x037f && c <= 0x1fff) ||
                     (c >= 0x200c && c <= 0x200d) ||
                     (c >= 0x2070 && c <= 0x218f) ||
                     (c >= 0x2c00 && c <= 0x2fef) ||
                     (c >= 0x3001 && c <= 0xd7ff) ||
                     (c >= 0xf900 && c <= 0xfdcf) ||
                     (c >= 0xfdf0 && c <= 0xfffd))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool IsPNCharsBase(char c, char d)
        {
            if (Char.IsHighSurrogate(c) && Char.IsLowSurrogate(d))
            {
                int codepoint = Char.ConvertToUtf32(c, d);
                return (codepoint >= 0x10000 && codepoint <= 0xeffff);
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
            //PN_CHARS	::=	PN_CHARS_U | '-' | [0-9] | #x00B7 | [#x0300-#x036F] | [#x203F-#x2040]
            if (c == '-')
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
            else if (IsPNCharsU(c))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool IsPNChars(char c, char d)
        {
            //PN_CHARS	::=	PN_CHARS_U | '-' | [0-9] | #x00B7 | [#x0300-#x036F] | [#x203F-#x2040]
            return IsPnCharsU(c, d);
        }

        /// <summary>
        /// Gets whether a character matches the PN_CHARS_U production from the Turtle specification
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsPNCharsU(char c)
        {
            //PN_CHARS_U	::=	PN_CHARS_BASE | '_'
            return c == '_' || IsPNCharsBase(c);
        }

        public static bool IsPnCharsU(char c, char d)
        {
            //PN_CHARS_U	::=	PN_CHARS_BASE | '_'
            return IsPNCharsBase(c, d);
        }

        #endregion

        #region Member Submission Turtle Character Productions

        public static bool IsNameStartChar(char c)
        {
            //[30]	nameStartChar	::=	[A-Z] | "_" | [a-z] | [#x00C0-#x00D6] | [#x00D8-#x00F6] | [#x00F8-#x02FF] | [#x0370-#x037D] | [#x037F-#x1FFF] | [#x200C-#x200D] | [#x2070-#x218F] | [#x2C00-#x2FEF] | [#x3001-#xD7FF] | [#xF900-#xFDCF] | [#xFDF0-#xFFFD] | [#x10000-#xEFFFF]
            if (c >= 'A' && c <= 'Z')
            {
                return true;
            }
            else if (c >= 'a' && c <= 'z')
            {
                return true;
            }
            else if (c == '_')
            {
                return true;
            }
            else if ((c >= 0x00c0 && c <= 0x00d6) ||
                     (c >= 0x00d8 && c <= 0x00f6) ||
                     (c >= 0x00f8 && c <= 0x02ff) ||
                     (c >= 0x0370 && c <= 0x037d) ||
                     (c >= 0x037f && c <= 0x1fff) ||
                     (c >= 0x200c && c <= 0x200d) ||
                     (c >= 0x2070 && c <= 0x218f) ||
                     (c >= 0x2c00 && c <= 0x2fef) ||
                     (c >= 0x3001 && c <= 0xd7ff) ||
                     (c >= 0xf900 && c <= 0xfdcf) ||
                     (c >= 0xfdf0 && c <= 0xfffd))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool IsNameStartChar(char c, char d)
        {
            if (Char.IsHighSurrogate(c) && Char.IsLowSurrogate(d))
            {
                int codepoint = Char.ConvertToUtf32(c, d);
                return (codepoint >= 0x10000 && codepoint <= 0xeffff);
            }
            else
            {
                return false;
            }
        }

        public static bool IsNameChar(char c)
        {
            //[31]	nameChar	::=	nameStartChar | '-' | [0-9] | #x00B7 | [#x0300-#x036F] | [#x203F-#x2040]
            if (c == '-')
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
            else if ((c >= 0x0300 && c <= 0x036f) || (c >= 0x203f && c <= 0x2040))
            {
                return true;
            }
            else
            {
                return IsNameStartChar(c);
            }
        }

        public static bool IsNameChar(char c, char d)
        {
            return IsNameStartChar(c, d);
        }


        #endregion
    }

}
