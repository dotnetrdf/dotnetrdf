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

namespace VDS.RDF.Parsing
{
    /// <summary>
    /// Static Helper class which can be used to validate IRIs according to <a href="http://www.ietf.org/rfc/rfc3987.txt">RFC 3987</a>
    /// </summary>
    /// <remarks>
    /// Some valid IRIs may be rejected by these validating functions as the IRI specification allows character codes which are outside the range of the .Net char type
    /// </remarks>
    public static class IriSpecsHelper
    {
        /// <summary>
        /// Gets whether a string matches the IRI production
        /// </summary>
        /// <param name="value">String</param>
        /// <returns></returns>
        public static bool IsIri(String value)
        {
            if (!value.Contains(':')) return false;
            String scheme = value.Substring(0, value.IndexOf(':'));
            String rest = value.Substring(value.IndexOf(':') + 1);
            if (rest.Contains("?"))
            {
                String part = rest.Substring(0, rest.IndexOf('?'));
                String queryAndFragment = rest.Substring(rest.IndexOf('?') + 1);
                String query, fragment;
                if (queryAndFragment.Contains('#'))
                {
                    query = queryAndFragment.Substring(0, queryAndFragment.IndexOf('#'));
                    fragment = queryAndFragment.Substring(queryAndFragment.IndexOf('#') + 1);
                    return IsScheme(scheme) && IsIHierPart(part) && IsIQuery(query) && IsIFragment(fragment);
                }
                else
                {
                    query = queryAndFragment;
                    return IsScheme(scheme) && IsIHierPart(part) && IsIQuery(query);
                }
            } 
            else if (rest.Contains('#'))
            {
                String part = rest.Substring(0, rest.IndexOf('#'));
                String fragment = rest.Substring(rest.IndexOf('#') + 1);
                return IsScheme(scheme) && IsIHierPart(part) && IsIFragment(fragment);
            }
            else
            {
                return IsScheme(scheme) && IsIHierPart(rest);
            }
        }

        /// <summary>
        /// Gets whether a string matches the ihier-part production
        /// </summary>
        /// <param name="value">String</param>
        /// <returns></returns>
        public static bool IsIHierPart(String value)
        {
            if (value.StartsWith("//"))
            {
                String reference = value.Substring(2);
                if (value.Contains("/"))
                {
                    String auth = value.Substring(0, value.IndexOf('/'));
                    String path = value.Substring(value.IndexOf('/') + 1);
                    return IsIAuthority(auth) && IsIPathAbEmpty(path);
                }
                else
                {
                    return IsIAuthority(reference);
                }
            }
            else
            {
                return IsIPathAbsolute(value) || IsIPathRootless(value) || IsIPathEmpty(value);
            }
        }

        /// <summary>
        /// Gets whether a string matches the IRI-reference production
        /// </summary>
        /// <param name="value">String</param>
        /// <returns></returns>
        public static bool IsIriReference(String value)
        {
            return IsIri(value) || IsIrelativeRef(value);
        }

        /// <summary>
        /// Gets whether a string matches the absolute-IRI production
        /// </summary>
        /// <param name="value">String</param>
        /// <returns></returns>
        public static bool IsAbsoluteIri(String value)
        {
            if (!value.Contains(":")) return false;
            String scheme = value.Substring(0, value.IndexOf(':'));
            String rest = value.Substring(value.IndexOf(':') + 1);
            if (rest.Contains("?"))
            {
                String part = rest.Substring(0, rest.IndexOf('?'));
                String query = rest.Substring(rest.IndexOf('?') + 1);
                return IsScheme(scheme) && IsIHierPart(part) && IsIQuery(query);
            }
            else
            {
                return IsScheme(scheme) && IsIHierPart(rest);
            }
        }

        /// <summary>
        /// Gets whether a string matches the irelative-ref production
        /// </summary>
        /// <param name="value">String</param>
        /// <returns></returns>
        public static bool IsIrelativeRef(String value)
        {
            if (value.Contains('?'))
            {
                String reference = value.Substring(0, value.IndexOf('?'));
                String rest = value.Substring(value.IndexOf('?') + 1);
                if (rest.Contains('#'))
                {
                    String query = rest.Substring(0, rest.IndexOf('#'));
                    String fragment = rest.Substring(rest.IndexOf('#') + 1);
                    return IsIrelativePart(reference) && IsIQuery(query) && IsIFragment(fragment);
                }
                else
                {
                    return IsIrelativePart(reference) && IsIQuery(rest);
                }
            }
            else if (value.Contains('#'))
            {
                String reference = value.Substring(0, value.IndexOf('#'));
                String fragment = value.Substring(value.IndexOf('#') + 1);
                return IsIrelativePart(reference) && IsIFragment(fragment);
            }
            else
            {
                return IsIrelativePart(value);
            }
        }

        /// <summary>
        /// Gets whether a string matches the irelative-part production
        /// </summary>
        /// <param name="value">String</param>
        /// <returns></returns>
        public static bool IsIrelativePart(String value)
        {
            if (value.StartsWith("//"))
            {
                String reference = value.Substring(2);
                if (value.Contains("/"))
                {
                    String auth = value.Substring(0, value.IndexOf('/'));
                    String path = value.Substring(value.IndexOf('/') + 1);
                    return IsIAuthority(auth) && IsIPathAbEmpty(path);
                }
                else
                {
                    return IsIAuthority(reference);
                }
            }
            else
            {
                return IsIPathAbsolute(value) || IsIPathNoScheme(value) || IsIPathEmpty(value);
            }
        }

        /// <summary>
        /// Gets whether a string matches the iauthority production
        /// </summary>
        /// <param name="value">String</param>
        /// <returns></returns>
        public static bool IsIAuthority(String value)
        {
            if (value.Contains("@"))
            {
                String userinfo = value.Substring(0, value.IndexOf('@'));
                String rest = value.Substring(value.IndexOf('@') + 1);
                if (rest.Contains(":"))
                {
                    String host = rest.Substring(0, rest.IndexOf(':'));
                    String port = rest.Substring(rest.IndexOf(':') + 1);
                    return IsIUserInfo(userinfo) && IsIHost(host) && IsPort(port);
                }
                else
                {
                    return IsIUserInfo(userinfo) && IsIHost(rest);
                }
            }
            else
            {
                if (value.Contains(":"))
                {
                    String host = value.Substring(0, value.IndexOf(':'));
                    String port = value.Substring(value.IndexOf(':') + 1);
                    return IsIHost(host) && IsPort(port);
                }
                else
                {
                    return IsIHost(value);
                }
            }
        }

        /// <summary>
        /// Gets whether a string matches the userinfo production
        /// </summary>
        /// <param name="value">String</param>
        /// <returns></returns>
        public static bool IsIUserInfo(String value)
        {
            char[] cs = value.ToCharArray();
            int i = 0;
            while (i < cs.Length)
            {
                if (cs[i] == ':')
                {
                    //OK
                }
                else if (cs[i] == '%')
                {
                    if (!IsPctEncoded(new String(new char[] { cs[i], cs[i + 1], cs[i + 2] }))) return false;
                    i += 2;
                }
                else if (!IsUnreserved(cs[i]) && !IsSubDelims(cs[i]))
                {
                    return false;
                }
                i++;
            }
            return true;
        }

        /// <summary>
        /// Gets whether a string matches the ihost production
        /// </summary>
        /// <param name="value">String</param>
        /// <returns></returns>
        public static bool IsIHost(String value)
        {
            return IsIPLiteral(value) || IsIPv4Address(value) || IsIRegName(value);
        }

        /// <summary>
        /// Gets whether a string matches the ireg-name production
        /// </summary>
        /// <param name="value">String</param>
        /// <returns></returns>
        public static bool IsIRegName(String value)
        {
            char[] cs = value.ToCharArray();
            int i = 0;
            while (i < cs.Length)
            {
                if (cs[i] == '%')
                {
                    if (!IsPctEncoded(new String(new char[] { cs[i], cs[i + 1], cs[i + 2] }))) return false;
                    i += 2;
                }
                else if (!IsUnreserved(cs[i]) && !IsSubDelims(cs[i]))
                {
                    return false;
                }
                i++;
            }
            return true;
        }

        /// <summary>
        /// Gets whether a string matches the ipath production
        /// </summary>
        /// <param name="value">String</param>
        /// <returns></returns>
        public static bool IsIPath(String value)
        {
            return IsIPathAbEmpty(value) ||
                   IsIPathAbsolute(value) ||
                   IsIPathNoScheme(value) ||
                   IsIPathRootless(value) ||
                   IsIPathEmpty(value);
        }

        /// <summary>
        /// Gets whether a string matches the ipath-abempty production
        /// </summary>
        /// <param name="value">String</param>
        /// <returns></returns>
        public static bool IsIPathAbEmpty(String value)
        {
            if (value.Equals(String.Empty)) return true;
            String[] segments = value.Split('/');
            return segments.All(s => IsISegment(s));
        }

        /// <summary>
        /// Gets whether a string matches the ipath-absolute production
        /// </summary>
        /// <param name="value">String</param>
        /// <returns></returns>
        public static bool IsIPathAbsolute(String value)
        {
            if (value.StartsWith("/"))
            {
                if (value.Equals("/"))
                {
                    return true;
                }
                else
                {
                    String[] segments = value.Substring(1).Split('/');
                    if (segments.Length == 1)
                    {
                        return IsISegmentNz(segments[0]);
                    }
                    else
                    {
                        return IsISegmentNz(segments[0]) && segments.Skip(1).All(s => IsISegment(s));
                    }
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gets whether a string matches the ipath-noscheme production
        /// </summary>
        /// <param name="value">String</param>
        /// <returns></returns>
        public static bool IsIPathNoScheme(String value)
        {
            if (value.Contains("/"))
            {
                String[] segments = value.Split('/');
                return IsISegmentNzNc(segments[0]) && segments.Skip(1).All(s => IsISegmentNzNc(s));
            }
            else
            {
                return IsISegmentNzNc(value);
            }
        }

        /// <summary>
        /// Gets whether a string matches the ipath-rootless production
        /// </summary>
        /// <param name="value">String</param>
        /// <returns></returns>
        public static bool IsIPathRootless(String value)
        {
            if (value.Contains("/"))
            {
                String[] segments = value.Split('/');
                return IsISegmentNz(segments[0]) && segments.Skip(1).All(s => IsISegment(s));
            }
            else
            {
                return IsISegmentNz(value);
            }
        }

        /// <summary>
        /// Gets whether a string matches the ipath-empty production
        /// </summary>
        /// <param name="value">String</param>
        /// <returns></returns>
        public static bool IsIPathEmpty(String value)
        {
            return value.Equals(String.Empty);
        }

        /// <summary>
        /// Gets whether a string matches the isegment production
        /// </summary>
        /// <param name="value">String</param>
        /// <returns></returns>
        public static bool IsISegment(String value)
        {
            if (value.Equals(String.Empty)) return true;
            return IsISegmentNz(value);
        }

        /// <summary>
        /// Gets whether a string matches the isegment-nz production
        /// </summary>
        /// <param name="value">String</param>
        /// <returns></returns>
        public static bool IsISegmentNz(String value)
        {
            if (value.Equals(String.Empty)) return false;

            char[] cs = value.ToCharArray();
            int i = 0;
            while (i < cs.Length)
            {
                if (cs[i] == '%')
                {
                    if (!IsIpChar(new String(new char[] { cs[i], cs[i + 1], cs[i + 2] }))) return false;
                    i += 2;
                }
                else
                {
                    if (!IsIpChar(new String(cs[i], 1))) return false;
                }
                i++;
            }
            return true;
        }

        /// <summary>
        /// Gets whether a string matches the isegment-nz-nc production
        /// </summary>
        /// <param name="value">String</param>
        /// <returns></returns>
        public static bool IsISegmentNzNc(String value)
        {
            if (value.Equals(String.Empty)) return false;

            char[] cs = value.ToCharArray();
            int i = 0;
            while (i < cs.Length)
            {
                if (cs[i] == '%')
                {
                    if (!IsIpChar(new String(new char[] { cs[i], cs[i + 1], cs[i + 2] }))) return false;
                    i += 2;
                }
                else
                {
                    if (!(cs[i] == '@' || IsSubDelims(cs[i]) || IsIUnreserved(cs[i]))) return false;
                }
                i++;
            }
            return true;
        }

        /// <summary>
        /// Gets whether a string matches the ipchar production
        /// </summary>
        /// <param name="value">String</param>
        /// <returns></returns>
        public static bool IsIpChar(String value)
        {
            if (value.Length == 1)
            {
                char c = value[0];
                if (c == ':' || c == '@')
                {
                    return true;
                }
                else if (IsSubDelims(c))
                {
                    return true;
                }
                else if (IsIUnreserved(c))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return IsPctEncoded(value);
            }
        }

        /// <summary>
        /// Gets whether a string matches the iquery production
        /// </summary>
        /// <param name="value">String</param>
        /// <returns></returns>
        public static bool IsIQuery(String value)
        {
            char[] cs = value.ToCharArray();
            int i = 0;
            while (i < cs.Length)
            {
                if (cs[i] == '/' || cs[i] == '?')
                {
                    //OK
                }
                else if (cs[i] == '%')
                {
                    if (!IsPctEncoded(new String(new char[] { cs[i], cs[i + 1], cs[i + 2] }))) return false;
                    i += 2;
                }
                else if (!IsIpChar(new String(cs[i], 1)))
                {
                    return false;
                }
                i++;
            }
            return true;
        }

        /// <summary>
        /// Gets whether a string matches the ifragment production
        /// </summary>
        /// <param name="value">String</param>
        /// <returns></returns>
        public static bool IsIFragment(String value)
        {
            char[] cs = value.ToCharArray();
            int i = 0;
            while (i < cs.Length)
            {
                if (cs[i] == '/' || cs[i] == '?')
                {
                    //OK
                }
                else if (cs[i] == '%')
                {
                    if (!IsPctEncoded(new String(new char[] {cs[i], cs[i + 1], cs[i + 2]}))) return false;
                    i += 2;
                }
                else if (!IsIpChar(new String(cs[i],1)))
                {
                    return false;
                }
                i++;
            }
            return true;
        }

        /// <summary>
        /// Gets whether a character matches the iunreserved production
        /// </summary>
        /// <param name="c">Character</param>
        /// <returns></returns>
        public static bool IsIUnreserved(char c)
        {
            if (Char.IsLetterOrDigit(c))
            {
                return true;
            } 
            else if (c == '-' || c == '.' || c == '_' || c == '~')
            {
                return true;
            }
            else if (IsUcsChar(c))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gets whether a character matches the ucschar production
        /// </summary>
        /// <param name="c">Character</param>
        /// <returns></returns>
        /// <remarks>
        /// Not all strings that will match the official ucschar production will be matched by this function as the ucschar production permits character codes beyond the range of the .Net char type
        /// </remarks>
        public static bool IsUcsChar(char c)
        {
            return UnicodeSpecsHelper.IsLetterOrDigit(c);
        }

        /// <summary>
        /// Gets whether a string matches the scheme production
        /// </summary>
        /// <param name="value">String</param>
        /// <returns></returns>
        public static bool IsScheme(String value)
        {
            if (value.Equals(String.Empty)) return false;

            char[] cs = value.ToCharArray();
            for (int i = 0; i < cs.Length; i++)
            {
                if (i == 0)
                {
                    if (!Char.IsLetter(cs[i])) return false;
                }
                else
                {
                    if (!Char.IsLetterOrDigit(cs[i]) && cs[i] != '+' && cs[i] != '-' && cs[i] != '.') return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Gets whether a string matches the port production
        /// </summary>
        /// <param name="value">String</param>
        /// <returns></returns>
        public static bool IsPort(String value)
        {
            return value.ToCharArray().All(c => Char.IsDigit(c));
        }

        /// <summary>
        /// Gets whether a string matches the IP-literal production
        /// </summary>
        /// <param name="value">String</param>
        /// <returns></returns>
        public static bool IsIPLiteral(String value)
        {
            if (value.StartsWith("[") && value.EndsWith("]"))
            {
                String literal = value.Substring(1, value.Length - 2);
                return IsIPv6Address(literal) || IsIPvFuture(literal);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gets whether a string matches the IPvFuture production
        /// </summary>
        /// <param name="value">String</param>
        /// <returns></returns>
        public static bool IsIPvFuture(String value)
        {
            //TODO: Implement IsIPvFuture
            return false;
        }

        /// <summary>
        /// Gets whether a string matches the IPv6address production
        /// </summary>
        /// <param name="value">String</param>
        /// <returns></returns>
        public static bool IsIPv6Address(String value)
        {
            if (value.Contains("::"))
            {
                String start = value.Substring(0, value.IndexOf("::"));
                String rest = value.Substring(value.IndexOf("::") + 2);
                String[] startChunks = start.Split(':');
                if (!startChunks.All(c => IsH16(c))) return false;
                switch (startChunks.Length)
                {
                    case 1:
                    case 2:
                    case 3:
                        String[] restChunks = rest.Split(new char[] { ':' }, 6 - startChunks.Length);
                        return restChunks.Take(restChunks.Length - 1).All(c => IsH16(c)) && IsLs32(restChunks[restChunks.Length - 1]);
                    case 4:
                        if (!rest.Contains(':')) return false;
                        return IsH16(rest.Substring(0, rest.IndexOf(':'))) && IsLs32(rest.Substring(rest.IndexOf(':') + 1));
                    case 5:
                        return IsLs32(rest);
                    case 6:
                        return IsH16(rest);
                    case 7:
                        return rest.Equals(String.Empty);
                    default:
                        return false;
                }
            }
            else
            {
                String[] chunks = value.Split(new char[] { ':' }, 7);
                if (chunks.Length < 7) return false;
                return chunks.Take(6).All(c => IsH16(c)) && IsLs32(chunks[6]);
            }
        }

        /// <summary>
        /// Gets whether a string matches the h16 production
        /// </summary>
        /// <param name="value">String</param>
        /// <returns></returns>
        public static bool IsH16(String value)
        {
            if (value.Length < 1 || value.Length > 4) return false;
            return value.ToCharArray().All(c => IsHexDigit(c));
        }

        /// <summary>
        /// Gets whether a string matches the ls32 production
        /// </summary>
        /// <param name="value">String</param>
        /// <returns></returns>
        public static bool IsLs32(String value)
        {
            if (value.Contains(":"))
            {
                String a = value.Substring(0, value.IndexOf(':'));
                String b = value.Substring(value.IndexOf(':') + 1);
                return IsH16(a) && IsH16(b);
            }
            else
            {
                return IsIPv4Address(value);
            }
        }

        /// <summary>
        /// Gets whether a string matches the IPv4address production
        /// </summary>
        /// <param name="value">String</param>
        /// <returns></returns>
        public static bool IsIPv4Address(String value)
        {
            if (value.Contains("."))
            {
                String[] octets = value.Split('.');
                if (octets.Length != 4) return false;
                return octets.All(o => IsDecOctet(o));
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gets whether a string matches the dec-octet production
        /// </summary>
        /// <param name="value">String</param>
        /// <returns></returns>
        public static bool IsDecOctet(String value)
        {
            switch (value.Length)
            {
                case 1:
                    return Char.IsDigit(value[0]);
                case 2:
                    return value.ToCharArray().All(c => Char.IsDigit(c));
                case 3:
                    return (value[0] == '1' || value[0] == '2') && 
                           (value[1] == '0' || value[1] == '1' || value[1] == '2' || value[1] == '3' || value[1] == '4' || value[1] == '5') &&
                           Char.IsDigit(value[2]);
                default:
                    return false;
            }
        }

        /// <summary>
        /// Gets whether a string matches the pct-encoded production
        /// </summary>
        /// <param name="value">String</param>
        /// <returns></returns>
        public static bool IsPctEncoded(String value)
        {
            if (value.StartsWith("%"))
            {
                if (value.Length == 3)
                {
                    return IsHexDigit(value[1]) && IsHexDigit(value[2]);
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gets whether a character matches the unreserved production
        /// </summary>
        /// <param name="c">Character</param>
        /// <returns></returns>
        public static bool IsUnreserved(char c)
        {
            if (Char.IsLetterOrDigit(c))
            {
                return true;
            }
            else
            {
                switch (c)
                {
                    case '-':
                    case '.':
                    case '_':
                    case '~':
                        return true;
                    default:
                        return false;
                }
            }
        }

        /// <summary>
        /// Gets whether a character matches the reserved production
        /// </summary>
        /// <param name="c">Character</param>
        /// <returns></returns>
        public static bool IsReserved(char c)
        {
            return IsGenDelims(c) || IsSubDelims(c);
        }

        /// <summary>
        /// Gets whether a character matches the gen-delims production
        /// </summary>
        /// <param name="c">Character</param>
        /// <returns></returns>
        public static bool IsGenDelims(char c)
        {
            switch (c)
            {
                case ':':
                case '/':
                case '?':
                case '#':
                case '[':
                case ']':
                case '@':
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Gets whether a character matches the sub-delims production
        /// </summary>
        /// <param name="c">Character</param>
        /// <returns></returns>
        public static bool IsSubDelims(char c)
        {
            switch (c)
            {
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
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Gets whether a character matches the HEXDIG terminal
        /// </summary>
        /// <param name="c">Character</param>
        /// <returns></returns>
        public static bool IsHexDigit(char c)
        {
            if (Char.IsDigit(c)) return true;
            switch (c)
            {
                case 'a':
                case 'A':
                case 'b':
                case 'B':
                case 'c':
                case 'C':
                case 'd':
                case 'D':
                case 'e':
                case 'E':
                case 'f':
                case 'F':
                    return true;
                default:
                    return false;
            }
        }
    }
}
