/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2025 dotNetRDF Project (http://dotnetrdf.org/)
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

using System.Linq;

namespace VDS.RDF.Parsing;

/// <summary>
/// Static Helper class which can be used to validate IRIs according to. <a href="http://www.ietf.org/rfc/rfc3987.txt">RFC 3987</a>
/// </summary>
/// <remarks>
/// Some valid IRIs may be rejected by these validating functions as the IRI specification allows character codes which are outside the range of the .Net char type.
/// </remarks>
public static class IriSpecsHelper
{
    /// <summary>
    /// Gets whether a string matches the IRI production.
    /// </summary>
    /// <param name="value">String.</param>
    /// <returns></returns>
    public static bool IsIri(string value)
    {
        if (!value.Contains(':'))
        {
            // No scheme so some form of relative IRI
            if (value.StartsWith("?"))
            {
                // Querystring and possibly a fragment
                if (value.Contains("#"))
                {
                    // Querystring and a fragment
                    var query = value.Substring(1, value.IndexOf('#'));
                    var fragment = value.Substring(query.Length + 2);
                    return IsIQuery(query) && IsIFragment(fragment);
                }
                else
                {
                    // Just a querystring
                    return IsIQuery(value.Substring(1));
                }
            }
            else if (value.Contains("?"))
            {
                // Path and querystring plus possibly a fragment
                var part = value.Substring(0, value.IndexOf('?'));
                var rest = value.Substring(part.Length + 1);
                if (rest.Contains("#"))
                {
                    // Has a path, querystring and fragment
                    var query = rest.Substring(0, rest.IndexOf('#'));
                    var fragment = rest.Substring(query.Length + 1);
                    return IsIPath(part) && IsIQuery(query) && IsIFragment(fragment);
                }
                else
                {
                    // Just a path and querystring
                    return IsIPath(part) && IsIQuery(rest);
                }
            }
            else if (value.StartsWith("#"))
            {
                // Just a fragment
                return IsIFragment(value.Substring(1));
            }
            else if (value.Contains("#"))
            {
                // Path and fragment
                var part = value.Substring(0, value.IndexOf('#'));
                var fragment = value.Substring(part.Length + 1);
                return IsIPath(part) && IsIFragment(fragment);
            }
            else
            {
                // Assume a relative path
                return IsIPath(value);
            }
        }
        else
        {
            // Has a scheme
            var scheme = value.Substring(0, value.IndexOf(':'));
            var rest = value.Substring(value.IndexOf(':') + 1);
            if (rest.Contains("?"))
            {
                // Has a path, querystring and possibly a fragment
                var part = rest.Substring(0, rest.IndexOf('?'));
                var queryAndFragment = rest.Substring(part.Length + 1);
                string query, fragment;
                if (queryAndFragment.Contains('#'))
                {
                    // Has a path, querystring and a fragment
                    query = queryAndFragment.Substring(0, queryAndFragment.IndexOf('#'));
                    fragment = queryAndFragment.Substring(queryAndFragment.IndexOf('#') + 1);
                    return IsScheme(scheme) && IsIHierPart(part) && IsIQuery(query) && IsIFragment(fragment);
                }
                else
                {
                    // Has a path and querystring
                    query = queryAndFragment;
                    return IsScheme(scheme) && IsIHierPart(part) && IsIQuery(query);
                }
            }
            else if (rest.Contains('#'))
            {
                // Has a path and fragment
                var part = rest.Substring(0, rest.IndexOf('#'));
                var fragment = rest.Substring(rest.IndexOf('#') + 1);
                return IsScheme(scheme) && IsIHierPart(part) && IsIFragment(fragment);
            }
            else
            {
                // Has a path
                return IsScheme(scheme) && IsIHierPart(rest);
            }
        }
    }

    /// <summary>
    /// Gets whether a string matches the ihier-part production.
    /// </summary>
    /// <param name="value">String.</param>
    /// <returns></returns>
    public static bool IsIHierPart(string value)
    {
        if (value.StartsWith("//"))
        {
            var reference = value.Substring(2);
            if (value.Contains("/"))
            {
                var auth = value.Substring(0, value.IndexOf('/'));
                var path = value.Substring(value.IndexOf('/') + 1);
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
    /// Gets whether a string matches the IRI-reference production.
    /// </summary>
    /// <param name="value">String.</param>
    /// <returns></returns>
    public static bool IsIriReference(string value)
    {
        return IsIri(value) || IsIrelativeRef(value);
    }

    /// <summary>
    /// Gets whether a string matches the absolute-IRI production.
    /// </summary>
    /// <param name="value">String.</param>
    /// <returns></returns>
    public static bool IsAbsoluteIri(string value)
    {
        if (!value.Contains(":")) return false;
        var scheme = value.Substring(0, value.IndexOf(':'));
        var rest = value.Substring(value.IndexOf(':') + 1);
        if (rest.Contains("?"))
        {
            var part = rest.Substring(0, rest.IndexOf('?'));
            var query = rest.Substring(rest.IndexOf('?') + 1);
            return IsScheme(scheme) && IsIHierPart(part) && IsIQuery(query);
        }
        else
        {
            return IsScheme(scheme) && IsIHierPart(rest);
        }
    }

    /// <summary>
    /// Gets whether a string matches the irelative-ref production.
    /// </summary>
    /// <param name="value">String.</param>
    /// <returns></returns>
    public static bool IsIrelativeRef(string value)
    {
        if (value.Contains('?'))
        {
            var reference = value.Substring(0, value.IndexOf('?'));
            var rest = value.Substring(value.IndexOf('?') + 1);
            if (rest.Contains('#'))
            {
                var query = rest.Substring(0, rest.IndexOf('#'));
                var fragment = rest.Substring(rest.IndexOf('#') + 1);
                return IsIrelativePart(reference) && IsIQuery(query) && IsIFragment(fragment);
            }
            else
            {
                return IsIrelativePart(reference) && IsIQuery(rest);
            }
        }
        else if (value.Contains('#'))
        {
            var reference = value.Substring(0, value.IndexOf('#'));
            var fragment = value.Substring(value.IndexOf('#') + 1);
            return IsIrelativePart(reference) && IsIFragment(fragment);
        }
        else
        {
            return IsIrelativePart(value);
        }
    }

    /// <summary>
    /// Gets whether a string matches the irelative-part production.
    /// </summary>
    /// <param name="value">String.</param>
    /// <returns></returns>
    public static bool IsIrelativePart(string value)
    {
        if (value.StartsWith("//"))
        {
            var reference = value.Substring(2);
            if (value.Contains("/"))
            {
                var auth = value.Substring(0, value.IndexOf('/'));
                var path = value.Substring(value.IndexOf('/') + 1);
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
    /// Gets whether a string matches the iauthority production.
    /// </summary>
    /// <param name="value">String.</param>
    /// <returns></returns>
    public static bool IsIAuthority(string value)
    {
        if (value.Contains("@"))
        {
            var userinfo = value.Substring(0, value.IndexOf('@'));
            var rest = value.Substring(value.IndexOf('@') + 1);
            if (rest.Contains(":"))
            {
                var host = rest.Substring(0, rest.IndexOf(':'));
                var port = rest.Substring(rest.IndexOf(':') + 1);
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
                var host = value.Substring(0, value.IndexOf(':'));
                var port = value.Substring(value.IndexOf(':') + 1);
                return IsIHost(host) && IsPort(port);
            }
            else
            {
                return IsIHost(value);
            }
        }
    }

    /// <summary>
    /// Gets whether a string matches the userinfo production.
    /// </summary>
    /// <param name="value">String.</param>
    /// <returns></returns>
    public static bool IsIUserInfo(string value)
    {
        var cs = value.ToCharArray();
        var i = 0;
        while (i < cs.Length)
        {
            if (cs[i] == ':')
            {
                // OK
            }
            else if (cs[i] == '%')
            {
                if (!IsPctEncoded(new string(new char[] { cs[i], cs[i + 1], cs[i + 2] }))) return false;
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
    /// Gets whether a string matches the ihost production.
    /// </summary>
    /// <param name="value">String.</param>
    /// <returns></returns>
    public static bool IsIHost(string value)
    {
        return IsIPLiteral(value) || IsIPv4Address(value) || IsIRegName(value);
    }

    /// <summary>
    /// Gets whether a string matches the ireg-name production.
    /// </summary>
    /// <param name="value">String.</param>
    /// <returns></returns>
    public static bool IsIRegName(string value)
    {
        var cs = value.ToCharArray();
        var i = 0;
        while (i < cs.Length)
        {
            if (cs[i] == '%')
            {
                if (!IsPctEncoded(new string(new char[] { cs[i], cs[i + 1], cs[i + 2] }))) return false;
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
    /// Gets whether a string matches the ipath production.
    /// </summary>
    /// <param name="value">String.</param>
    /// <returns></returns>
    public static bool IsIPath(string value)
    {
        return IsIPathAbEmpty(value) ||
               IsIPathAbsolute(value) ||
               IsIPathNoScheme(value) ||
               IsIPathRootless(value) ||
               IsIPathEmpty(value);
    }

    /// <summary>
    /// Gets whether a string matches the ipath-abempty production.
    /// </summary>
    /// <param name="value">String.</param>
    /// <returns></returns>
    public static bool IsIPathAbEmpty(string value)
    {
        if (value.Equals(string.Empty)) return true;
        var segments = value.Split('/');
        return segments.All(s => IsISegment(s));
    }

    /// <summary>
    /// Gets whether a string matches the ipath-absolute production.
    /// </summary>
    /// <param name="value">String.</param>
    /// <returns></returns>
    public static bool IsIPathAbsolute(string value)
    {
        if (value.StartsWith("/"))
        {
            if (value.Equals("/"))
            {
                return true;
            }
            else
            {
                var segments = value.Substring(1).Split('/');
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
    /// Gets whether a string matches the ipath-noscheme production.
    /// </summary>
    /// <param name="value">String.</param>
    /// <returns></returns>
    public static bool IsIPathNoScheme(string value)
    {
        if (value.Contains("/"))
        {
            var segments = value.Split('/');
            return IsISegmentNzNc(segments[0]) && segments.Skip(1).All(s => IsISegmentNzNc(s));
        }
        else
        {
            return IsISegmentNzNc(value);
        }
    }

    /// <summary>
    /// Gets whether a string matches the ipath-rootless production.
    /// </summary>
    /// <param name="value">String.</param>
    /// <returns></returns>
    public static bool IsIPathRootless(string value)
    {
        if (value.Contains("/"))
        {
            var segments = value.Split('/');
            return IsISegmentNz(segments[0]) && segments.Skip(1).All(s => IsISegment(s));
        }
        else
        {
            return IsISegmentNz(value);
        }
    }

    /// <summary>
    /// Gets whether a string matches the ipath-empty production.
    /// </summary>
    /// <param name="value">String.</param>
    /// <returns></returns>
    public static bool IsIPathEmpty(string value)
    {
        return value.Equals(string.Empty);
    }

    /// <summary>
    /// Gets whether a string matches the isegment production.
    /// </summary>
    /// <param name="value">String.</param>
    /// <returns></returns>
    public static bool IsISegment(string value)
    {
        if (value.Equals(string.Empty)) return true;
        return IsISegmentNz(value);
    }

    /// <summary>
    /// Gets whether a string matches the isegment-nz production.
    /// </summary>
    /// <param name="value">String.</param>
    /// <returns></returns>
    public static bool IsISegmentNz(string value)
    {
        if (value.Equals(string.Empty)) return false;

        var cs = value.ToCharArray();
        var i = 0;
        while (i < cs.Length)
        {
            if (cs[i] == '%')
            {
                if (!IsIpChar(new string(new char[] { cs[i], cs[i + 1], cs[i + 2] }))) return false;
                i += 2;
            }
            else
            {
                if (!IsIpChar(new string(cs[i], 1))) return false;
            }
            i++;
        }
        return true;
    }

    /// <summary>
    /// Gets whether a string matches the isegment-nz-nc production.
    /// </summary>
    /// <param name="value">String.</param>
    /// <returns></returns>
    public static bool IsISegmentNzNc(string value)
    {
        if (value.Equals(string.Empty)) return false;

        var cs = value.ToCharArray();
        var i = 0;
        while (i < cs.Length)
        {
            if (cs[i] == '%')
            {
                if (!IsIpChar(new string(new char[] { cs[i], cs[i + 1], cs[i + 2] }))) return false;
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
    /// Gets whether a string matches the ipchar production.
    /// </summary>
    /// <param name="value">String.</param>
    /// <returns></returns>
    public static bool IsIpChar(string value)
    {
        if (value.Length == 1)
        {
            var c = value[0];
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
    /// Gets whether a string matches the iquery production.
    /// </summary>
    /// <param name="value">String.</param>
    /// <returns></returns>
    public static bool IsIQuery(string value)
    {
        var cs = value.ToCharArray();
        var i = 0;
        while (i < cs.Length)
        {
            if (cs[i] == '/' || cs[i] == '?')
            {
                // OK
            }
            else if (cs[i] == '%')
            {
                if (!IsPctEncoded(new string(new char[] { cs[i], cs[i + 1], cs[i + 2] }))) return false;
                i += 2;
            }
            else if (!IsIpChar(new string(cs[i], 1)))
            {
                return false;
            }
            i++;
        }
        return true;
    }

    /// <summary>
    /// Gets whether a string matches the ifragment production.
    /// </summary>
    /// <param name="value">String.</param>
    /// <returns></returns>
    public static bool IsIFragment(string value)
    {
        var cs = value.ToCharArray();
        var i = 0;
        while (i < cs.Length)
        {
            if (cs[i] == '/' || cs[i] == '?')
            {
                // OK
            }
            else if (cs[i] == '%')
            {
                if (!IsPctEncoded(new string(new char[] {cs[i], cs[i + 1], cs[i + 2]}))) return false;
                i += 2;
            }
            else if (!IsIpChar(new string(cs[i],1)))
            {
                return false;
            }
            i++;
        }
        return true;
    }

    /// <summary>
    /// Gets whether a character matches the iunreserved production.
    /// </summary>
    /// <param name="c">Character.</param>
    /// <returns></returns>
    public static bool IsIUnreserved(char c)
    {
        if (char.IsLetterOrDigit(c))
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
    /// Gets whether a character matches the ucschar production.
    /// </summary>
    /// <param name="c">Character.</param>
    /// <returns></returns>
    /// <remarks>
    /// Not all strings that will match the official ucschar production will be matched by this function as the ucschar production permits character codes beyond the range of the .Net char type.
    /// </remarks>
    public static bool IsUcsChar(char c)
    {
        return UnicodeSpecsHelper.IsLetterOrDigit(c);
    }

    /// <summary>
    /// Gets whether a string matches the scheme production.
    /// </summary>
    /// <param name="value">String.</param>
    /// <returns></returns>
    public static bool IsScheme(string value)
    {
        if (value.Equals(string.Empty)) return false;

        var cs = value.ToCharArray();
        for (var i = 0; i < cs.Length; i++)
        {
            if (i == 0)
            {
                if (!char.IsLetter(cs[i])) return false;
            }
            else
            {
                if (!char.IsLetterOrDigit(cs[i]) && cs[i] != '+' && cs[i] != '-' && cs[i] != '.') return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Gets whether a string matches the port production.
    /// </summary>
    /// <param name="value">String.</param>
    /// <returns></returns>
    public static bool IsPort(string value)
    {
        return value.ToCharArray().All(c => char.IsDigit(c));
    }

    /// <summary>
    /// Gets whether a string matches the IP-literal production.
    /// </summary>
    /// <param name="value">String.</param>
    /// <returns></returns>
    public static bool IsIPLiteral(string value)
    {
        if (value.StartsWith("[") && value.EndsWith("]"))
        {
            var literal = value.Substring(1, value.Length - 2);
            return IsIPv6Address(literal) || IsIPvFuture(literal);
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Gets whether a string matches the IPvFuture production.
    /// </summary>
    /// <param name="value">String.</param>
    /// <returns></returns>
    public static bool IsIPvFuture(string value)
    {
        // TODO: Implement IsIPvFuture
        return false;
    }

    /// <summary>
    /// Gets whether a string matches the IPv6address production.
    /// </summary>
    /// <param name="value">String.</param>
    /// <returns></returns>
    public static bool IsIPv6Address(string value)
    {
        if (value.Contains("::"))
        {
            var start = value.Substring(0, value.IndexOf("::"));
            var rest = value.Substring(value.IndexOf("::") + 2);
            var startChunks = start.Split(':');
            if (!startChunks.All(c => IsH16(c))) return false;
            switch (startChunks.Length)
            {
                case 1:
                case 2:
                case 3:
                    var restChunks = rest.Split(new char[] { ':' }, 6 - startChunks.Length);
                    return restChunks.Take(restChunks.Length - 1).All(c => IsH16(c)) && IsLs32(restChunks[restChunks.Length - 1]);
                case 4:
                    if (!rest.Contains(':')) return false;
                    return IsH16(rest.Substring(0, rest.IndexOf(':'))) && IsLs32(rest.Substring(rest.IndexOf(':') + 1));
                case 5:
                    return IsLs32(rest);
                case 6:
                    return IsH16(rest);
                case 7:
                    return rest.Equals(string.Empty);
                default:
                    return false;
            }
        }
        else
        {
            var chunks = value.Split(new char[] { ':' }, 7);
            if (chunks.Length < 7) return false;
            return chunks.Take(6).All(c => IsH16(c)) && IsLs32(chunks[6]);
        }
    }

    /// <summary>
    /// Gets whether a string matches the h16 production.
    /// </summary>
    /// <param name="value">String.</param>
    /// <returns></returns>
    public static bool IsH16(string value)
    {
        if (value.Length < 1 || value.Length > 4) return false;
        return value.ToCharArray().All(c => IsHexDigit(c));
    }

    /// <summary>
    /// Gets whether a string matches the ls32 production.
    /// </summary>
    /// <param name="value">String.</param>
    /// <returns></returns>
    public static bool IsLs32(string value)
    {
        if (value.Contains(":"))
        {
            var a = value.Substring(0, value.IndexOf(':'));
            var b = value.Substring(value.IndexOf(':') + 1);
            return IsH16(a) && IsH16(b);
        }
        else
        {
            return IsIPv4Address(value);
        }
    }

    /// <summary>
    /// Gets whether a string matches the IPv4address production.
    /// </summary>
    /// <param name="value">String.</param>
    /// <returns></returns>
    public static bool IsIPv4Address(string value)
    {
        if (value.Contains("."))
        {
            var octets = value.Split('.');
            if (octets.Length != 4) return false;
            return octets.All(o => IsDecOctet(o));
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Gets whether a string matches the dec-octet production.
    /// </summary>
    /// <param name="value">String.</param>
    /// <returns></returns>
    public static bool IsDecOctet(string value)
    {
        switch (value.Length)
        {
            case 1:
                return char.IsDigit(value[0]);
            case 2:
                return value.ToCharArray().All(c => char.IsDigit(c));
            case 3:
                return (value[0] == '1' || value[0] == '2') && 
                       (value[1] == '0' || value[1] == '1' || value[1] == '2' || value[1] == '3' || value[1] == '4' || value[1] == '5') &&
                       char.IsDigit(value[2]);
            default:
                return false;
        }
    }

    /// <summary>
    /// Gets whether a string matches the pct-encoded production.
    /// </summary>
    /// <param name="value">String.</param>
    /// <returns></returns>
    public static bool IsPctEncoded(string value)
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
    /// Gets whether a character matches the unreserved production.
    /// </summary>
    /// <param name="c">Character.</param>
    /// <returns></returns>
    public static bool IsUnreserved(char c)
    {
        if (char.IsLetterOrDigit(c))
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
    /// Gets whether a character matches the reserved production.
    /// </summary>
    /// <param name="c">Character.</param>
    /// <returns></returns>
    public static bool IsReserved(char c)
    {
        return IsGenDelims(c) || IsSubDelims(c);
    }

    /// <summary>
    /// Gets whether a character matches the gen-delims production.
    /// </summary>
    /// <param name="c">Character.</param>
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
    /// Gets whether a character matches the sub-delims production.
    /// </summary>
    /// <param name="c">Character.</param>
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
    /// Gets whether a character matches the HEXDIG terminal.
    /// </summary>
    /// <param name="c">Character.</param>
    /// <returns></returns>
    public static bool IsHexDigit(char c)
    {
        if (char.IsDigit(c)) return true;
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
