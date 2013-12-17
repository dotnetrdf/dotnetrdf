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

namespace VDS.RDF.Utilities.StoreManager
{
    /// <summary>
    /// Extension Methods
    /// </summary>
    internal static class Extensions
    {
        /// <summary>
        /// Gets the Safe string form of an object
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns>The result of calling ToString() on non-null objects and String.Empty otherwise</returns>
        internal static String ToSafeString(this Object obj)
        {
            return (obj != null ? obj.ToString() : String.Empty);
        }

        /// <summary>
        /// Gets the Safe string form of a URI
        /// </summary>
        /// <param name="u">URI</param>
        /// <returns>Either the AbolsuteUri or an empty string</returns>
        internal static String ToSafeString(this Uri u)
        {
            return (u != null ? u.AbsoluteUri : String.Empty);
        }
    }
}
