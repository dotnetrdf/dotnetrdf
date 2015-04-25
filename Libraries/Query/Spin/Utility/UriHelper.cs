/*

dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

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

namespace VDS.RDF.Query.Spin.Utility
{
    /// <summary>
    /// Add support methods for a iis: uri scheme
    /// </summary>
    public static class UriHelper
    {
        /// <summary>
        /// Translates iis-schemed URIs into a local file-system based URI, otherwise returns the Uri unchanged
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static Uri ResolveUri(String uri)
        {
            return ResolveUri(UriFactory.Create(uri));
        }

        /// <summary>
        /// Translates iis-schemed URIs into a local file-system based URI, otherwise returns the Uri unchanged
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static Uri ResolveUri(Uri uri)
        {
            if (uri == null) return null;
            if (uri.Scheme == "iis")
            {
                uri = new Uri("file:///" + System.Web.Hosting.HostingEnvironment.MapPath(uri.AbsolutePath));
            }
            return uri;
        }
    }
}