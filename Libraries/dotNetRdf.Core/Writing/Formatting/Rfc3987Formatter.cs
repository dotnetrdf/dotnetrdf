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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace VDS.RDF.Writing.Formatting;

internal static class Rfc3987Formatter
{
    private class RuneEnumerator(string input) : IEnumerator<int>
    {
        private int _index = -1;

        public bool MoveNext()
        {
            if (++_index >= input.Length)
            {
                return false;
            }

            Current = Char.ConvertToUtf32(input, _index);
            if (Char.IsSurrogatePair(input, _index))
            {
                _index++;
            }

            return true;
        }

        public void Reset()
        {
            _index = -1;
        }

        public int Current { get; private set; }

        object IEnumerator.Current => Current;

        public void Dispose()
        {
            input = null;
        }
    }

    private static bool IsHexChar(int c)
    {
        return c is >= '0' and <= '9' or >= 'A' and <= 'F' or >= 'a' and <= 'f';
    }
    public static string EscapeUriString(string uriString)
    {
        var builder = new StringBuilder();
        using (var enumerator = new RuneEnumerator(uriString))
        {
            while (enumerator.MoveNext())
            {
                var c = enumerator.Current;
                if (c == '%')
                {
                    // Possibly already escaped
                    int? escaped1 = null;
                    int? escaped2 = null;
                    if (enumerator.MoveNext())
                    {
                        escaped1 = enumerator.Current;
                    }
                    else
                    {
                        builder.AppendEscaped(c);
                        continue;
                    }

                    if (!IsHexChar(escaped1.Value))
                    {
                        builder.AppendEscaped(c);
                        builder.AppendEscaped(escaped1.Value);
                        continue;
                    }

                    if (enumerator.MoveNext())
                    {
                        escaped2 = enumerator.Current;
                    }
                    else
                    {
                        builder.AppendEscaped(c);
                        builder.AppendEscaped(escaped1.Value);
                        continue;
                    }

                    if (!IsHexChar(escaped2.Value))
                    {
                        builder.AppendEscaped(c);
                        builder.AppendEscaped(escaped1.Value);
                        builder.AppendEscaped(escaped2.Value);
                    }

                    builder.AppendFormat("%{0}{1}", Char.ConvertFromUtf32(escaped1.Value), Char.ConvertFromUtf32(escaped2.Value));
                }
                else
                {
                    builder.AppendEscaped(c);
                }
            }
        }

        return builder.ToString();
    }

    private static void AppendEscaped(this StringBuilder builder, int c)
    {
        var s = Char.ConvertFromUtf32(c);
        if (c >= 0x30 && c <= 0x39)
        {
             // DIGIT
             builder.Append(s);
        }
        else if (c >= 0x041 && c <= 0x5a)
        {
            // ALPHA
            builder.Append(s);
        }
        else if (c >= 0x61 && c <= 0x7a)
        {
            // alpha
            builder.Append(s);
        }
        else if (c == '-' || c == '.' || c == '_' || c == '~')
        {
            // unreserved
            builder.Append(s);
        }
        else if (c == ':' || c == '/' || c == '?' || c == '#' || c == '[' || c == ']' || c == '@' ||
          c == '!' || c == '$' || c == '&' || c == '\'' || c == '(' || c == ')' || c == '*' ||
          c == '+' || c == ',' || c == ';' || c == '=')
        {
            // reserved
            builder.Append(s);
        }
        else if (c >= 0xA0 && c <= 0xD7FF || c >= 0xF900 && c <= 0xFDCF || c >= 0xFDF0 && c <= 0xFFEF ||
          c >= 0x10000 && c <= 0x1FFFD || c >= 0x20000 && c <= 0x2FFFD || c >= 0x30000 && c <= 0x3FFFD
          || c >= 0x40000 && c <= 0x4FFFD || c >= 0x50000 && c <= 0x5FFFD || c >= 0x60000 && c <= 0x6FFFD
          || c >= 0x70000 && c <= 0x7FFFD || c >= 0x80000 && c <= 0x8FFFD || c >= 0x90000 && c <= 0x9FFFD
          || c >= 0xA0000 && c <= 0xAFFFD || c >= 0xB0000 && c <= 0xBFFFD || c >= 0xC0000 && c <= 0xCFFFD
          || c >= 0xD0000 && c <= 0xDFFFD || c >= 0xE1000 && c <= 0xEFFFD)
        {
            builder.Append(s);
        }
        else if (c >= 0xE000 && c <= 0xF8FF || c >= 0xF0000 && c <= 0xFFFFD || c >= 0x100000 && c <= 0x10FFFD)
        {
            builder.Append(s);
        }
        else
        {
            foreach (var b in Encoding.UTF8.GetBytes(s))
            {
                builder.Append('%');
                builder.AppendFormat("{0:X2}", b);
            }
        }
    }
}