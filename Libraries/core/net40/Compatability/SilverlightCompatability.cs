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

#if SILVERLIGHT

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace VDS.RDF
{

    /// <summary>
    /// Extension Methods for the Silverlight build to avoid having to do lots of #if SILVERLIGHT blocks
    /// </summary>
    public static class SilverlightExtensions
    {
        /// <summary>
        /// Splits a String
        /// </summary>
        /// <param name="value">String to split</param>
        /// <param name="chars">Separator</param>
        /// <param name="count">Maximum number of results</param>
        /// <returns></returns>
        public static string[] Split(this string value, char[] chars, int count)
        {
            int start = 0;
            var ix = value.IndexOfAny(chars);
            var ret = new List<string>();
            while (ix >= 0 && ret.Count < (count - 1))
            {
                ret.Add(value.Substring(start, ix - start));
                start = ix + 1;
                ix = value.IndexOfAny(chars, start);
            }
            ret.Add(start < value.Length ? value.Substring(start) : String.Empty);
            return ret.ToArray();
            //String[] items = value.Split(chars);
            //return items.Length > count ? items.Take(count-1).Concat(String.Join(new String(chars), items, count, items.Length-(count-1)).AsEnumerable()).ToArray() : items;
        }

        /// <summary>
        /// Copies the characters in a specified substring to a character array
        /// </summary>
        /// <param name="value">String</param>
        /// <param name="startIndex">Start Index</param>
        /// <param name="count">Number of characters to copy</param>
        /// <returns></returns>
        public static char[] ToCharArray(this string value, int startIndex, int count)
        {
            char[] result = new char[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = value[startIndex + i];
            }
            return result;
        }

        /// <summary>
        /// Gets whether a URI is a file:/// URI
        /// </summary>
        /// <param name="u">URI</param>
        /// <returns></returns>
        public static bool IsFile(this Uri u)
        {
#if PORTABLE
            return u.Scheme.Equals("file", StringComparison.OrdinalIgnoreCase);
#else
            return u.Scheme.Equals(Uri.UriSchemeFile);
#endif
        }

        /// <summary>
        /// Gets the Segments of the URI
        /// </summary>
        /// <param name="u">URI</param>
        /// <returns></returns>
        public static String[] Segments(this Uri u)
        {
            String path = u.AbsolutePath;
            if (path.Equals("/"))
            {
                return new String[] { "/" };
            }
            else
            {
                String[] segments = path.Split('/');
                for (int i = 0; i < segments.Length - 1; i++)
                {
                    segments[i] += "/";
                }
                return segments;
            }
        }

        /// <summary>
        /// Gets the Path and Query of the URI
        /// </summary>
        /// <param name="u">URI</param>
        /// <returns></returns>
        public static String PathAndQuery(this Uri u)
        {
            return u.AbsolutePath + u.Query;
        }

        /// <summary>
        /// Removes all items from a List which return true for the given function
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="l">List</param>
        /// <param name="func">Function</param>
        public static void RemoveAll<T>(this List<T> l, Func<T,bool> func)
        {
            for (int i = 0; i < l.Count; i++)
            {
                if (func(l[i]))
                {
                    l.RemoveAt(i);
                    i--;
                }
            }
        }

        public static int FindIndex<T>(this List<T> list, Predicate<T> match)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (match(list[i])) return i;
            }
            return -1;
        }
    }
}

#endif
