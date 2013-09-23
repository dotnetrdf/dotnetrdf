using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Specifications
{
    public static class SparqlGrammarHelper
    {
        #region QName and Variable Name Validation

        /// <summary>
        /// Checks whether a given QName is valid in Sparql
        /// </summary>
        /// <param name="value">QName to check</param>
        /// <param name="syntax">SPARQL Syntax</param>
        /// <returns></returns>
        public static bool IsValidQName(String value, SparqlQuerySyntax syntax)
        {
            if (!value.Contains(':'))
            {
                //Must have a Colon in a QName
                return false;
            }
            else
            {
                //Split into Prefix and Local Name
                String[] parts = value.Split(':');

                //If SPARQL 1.0 then can only have two sections
                if (syntax == SparqlQuerySyntax.Sparql_1_0 && parts.Length > 2) return false;

                //All sections ending in a colon (i.e. all but the last) must match PN_PREFIX production
                for (int i = 0; i < parts.Length - 1; i++)
                {
                    if (!IsPNPrefix(parts[i].ToCharArray())) return false;
                }
                //Final section must match PN_LOCAL
                return IsPNLocal(parts[parts.Length - 1].ToCharArray(), syntax);
            }
        }

        /// <summary>
        /// Checks whether a given Variable Name is valid in Sparql
        /// </summary>
        /// <param name="value">Variable Name to check</param>
        /// <returns></returns>
        public static bool IsValidVarName(String value)
        {
            char[] cs = value.ToCharArray(1, value.Length - 1);

            //Variable Names can't be empty
            if (cs.Length == 0)
            {
                return false;
            }

            //First Character must be from PN_CHARS_U or a digit
            char first = cs[0];
            if (Char.IsDigit(first) || IsPNCharsU(first))
            {
                if (cs.Length > 1)
                {
                    for (int i = 1; i < cs.Length; i++)
                    {
                        if (i < cs.Length - 1)
                        {
                            //Subsequent Chars must be from PN_CHARS (except -) or a '.'
                            if (cs[i] == '.' || cs[i] == '-') return false;
                            if (!IsPNChars(cs[i])) return false;
                        }
                    }
                    return true;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gets whether a given prefix declaration is valid in SPARQL
        /// </summary>
        /// <param name="value">Prefix declaration</param>
        /// <returns></returns>
        public static bool IsValidPrefix(String value)
        {
            //Empty string is not a valid prefix
            if (value.Length == 0) return false;
            //Prefix must end with a colon
            if (!value.EndsWith(":")) return false;
            //Empty prefix is valid
            if (value.Length == 1) return true;
            //Otherwise must match IsPNPrefix() production
            //Remember to remove the terminating : which we have already validated
            return IsPNPrefix(value.Substring(0, value.Length - 1).ToCharArray());
        }

        /// <summary>
        /// Gets whether a given BNode ID is valid
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns></returns>
        public static bool IsValidBNode(String value)
        {
            //Must be at least 3 characters
            if (value.Length < 3) return false;
            //Must start with _:
            if (!value.StartsWith("_:")) return false;

            char[] cs = value.Substring(2).ToCharArray();

            //First character must be PN_CHARS_U or digit
            if (!Char.IsDigit(cs[0]) && !IsPNCharsU(cs[0])) return false;

            //If only one character it's a valid identifier since we've validated the first character
            if (cs.Length == 1) return true;

            //Otherwise we need to validate the rest of the identifier
            for (int i = 1; i < cs.Length; i++)
            {
                if (i < cs.Length - 1)
                {
                    //Middle characters may be PN_CHARS or a .
                    if (cs[i] != '.' && !IsPNChars(cs[i])) return false;
                }
                else
                {
                    //Final character must be in PN_CHARS
                    return IsPNChars(cs[i]);
                }
            }
            //Should be impossible to get here but must keep the compiler happy
            return false;
        }

        /// <summary>
        /// Checks whether a given Character matches the PN_CHARS_BASE rule from the Sparql Specification
        /// </summary>
        /// <param name="c">Character to test</param>
        /// <returns></returns>
        public static bool IsPNCharsBase(char c)
        {
            if (c >= 'A' && c <= 'Z')
            {
                return true;
            }
            else if (c >= 'a' && c <= 'z')
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
        /// Checks whether a given Character matches the PN_CHARS_U rule from the SPARQL Specification
        /// </summary>
        /// <param name="c">Character to test</param>
        /// <returns></returns>
        public static bool IsPNCharsU(char c)
        {
            return (c == '_' || IsPNCharsBase(c));
        }

        /// <summary>
        /// Checks whether a given Character matches the PN_CHARS rule from the SPARQL Specification
        /// </summary>
        /// <param name="c">Character to test</param>
        /// <returns></returns>
        public static bool IsPNChars(char c)
        {
            if (c == '-' || Char.IsDigit(c))
            {
                return true;
            }
            else if (c == 0x00b7)
            {
                return true;
            }
            else if (IsPNCharsU(c))
            {
                return true;
            }
            else if ((c >= 0x0300 && c <= 0x036f) ||
                     (c >= 0x204f && c <= 0x2040))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Checks whether a given String matches the PN_LOCAL rule from the Sparql Specification
        /// </summary>
        /// <param name="cs">String as character array</param>
        /// <param name="syntax">SPARQL Syntax</param>
        /// <returns></returns>
        public static bool IsPNLocal(char[] cs, SparqlQuerySyntax syntax)
        {
            if (cs.Length == 0)
            {
                //Empty Local Names are valid
                return true;
            }

            //First character must be a digit or from PN_CHARS_U
            char first = cs[0];
            int start = 0;
            if (Char.IsDigit(first) || IsPNCharsU(first) ||
                (syntax != SparqlQuerySyntax.Sparql_1_0 && IsPLX(cs, 0, out start)))
            {
                if (start > 0)
                {
                    //Means the first thing was a PLX
                    //If the only thing in the local name was a PLX this is valid
                    if (start == cs.Length - 1) return true;
                    //If there are further characters we'll start 
                }
                else
                {
                    //Otherwise we need to check the rest of the characters
                    start = 1;
                }

                //Check the rest of the characters
                if (cs.Length > start)
                {
                    for (int i = start; i < cs.Length; i++)
                    {
                        if (i < cs.Length - 1)
                        {
                            //Middle characters may be from PN_CHARS or '.'
                            int j = i;
                            if (!(cs[i] == '.' || IsPNChars(cs[i]) ||
                                  (syntax != SparqlQuerySyntax.Sparql_1_0 && IsPLX(cs, i, out j))
                                ))
                            {
                                return false;
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
                        else
                        {
                            //Last Character must be from PN_CHARS if it wasn't a PLX which is handled elsewhere
                            return IsPNChars(cs[i]);
                        }
                    }

                    //Should never get here but have to add this to keep compiler happy
                    throw new RdfParseException("Local Name validation error in SparqlSpecsHelper.IsPNLocal(char[] cs)");
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Checks whether a given String matches the PN_PREFIX rule from the SPARQL Specification
        /// </summary>
        /// <param name="cs">String as character array</param>
        /// <returns></returns>
        public static bool IsPNPrefix(char[] cs)
        {
            //Empty Prefixes are valid
            if (cs.Length == 0) return true;

            //First character must be from PN_CHARS_BASE
            char first = cs[0];
            if (IsPNCharsBase(first))
            {
                if (cs.Length > 1)
                {
                    for (int i = 1; i < cs.Length; i++)
                    {
                        if (i < cs.Length - 1)
                        {
                            //Middle characters may be from PN_CHARS or '.'
                            if (!(cs[i] == '.' || IsPNChars(cs[i])))
                            {
                                return false;
                            }
                        }
                        else
                        {
                            //Last Character must be from PN_CHARS
                            return IsPNChars(cs[i]);
                        }
                    }

                    //Should never get here but have to add this to keep compiler happy
                    throw new RdfParseException("Namespace Prefix validation error in SparqlSpecsHelper.IsPNPrefix(char[] cs)");
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Checks whether a given String matches the PLX rule from the SPARQL Specification
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
                if (startIndex >= cs.Length - 2)
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
        /// Unescapes local name escapes from QNames
        /// </summary>
        /// <param name="value">Value to unescape</param>
        /// <returns></returns>
        public static String UnescapeQName(String value)
        {
            if (value.Contains('\\') || value.Contains('%'))
            {
                StringBuilder output = new StringBuilder();
                output.Append(value.Substring(0, value.IndexOf(':')));
                char[] cs = value.ToCharArray();
                for (int i = output.Length; i < cs.Length; i++)
                {
                    if (cs[i] == '\\')
                    {
                        if (i == cs.Length - 1) throw new RdfParseException("Invalid backslash to start an escape at the end of the Local Name, expecting a single character after the backslash");
                        char esc = cs[i + 1];
                        switch (esc)
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
                                output.Append(esc);
                                i++;
                                break;
                            default:
                                throw new RdfParseException("Invalid character after a backslash, a backslash can only be used to escape a limited set (_~-.|$&\\()*+,;=/?#@%) of characters in a Local Name");
                        }
                    }
                    else if (cs[i] == '%')
                    {
                        //Remember that we are supposed to preserve precent encoded characters as-is
                        //Simply need to validate that they are valid encoding
                        if (i > cs.Length - 2)
                        {
                            throw new RdfParseException("Invalid % to start a percent encoded character in a Local Name, two hex digits are required after a %, use \\% to denote a percent character directly");
                        }
                        else
                        {
                            if (!IsHex(cs[i + 1]) || !IsHex(cs[i + 2]))
                            {
                                throw new RdfParseException("Invalid % encoding, % character was not followed by two hex digits, use \\% to denote a percent character directly");
                            }
                            else
                            {
                                output.Append(cs, i, 3);
                                i += 2;
                            }
                        }
                    }
                    else
                    {
                        output.Append(cs[i]);
                    }
                }
                return output.ToString();
            }
            else
            {
                return value;
            }
        }

        #endregion
    }
}
