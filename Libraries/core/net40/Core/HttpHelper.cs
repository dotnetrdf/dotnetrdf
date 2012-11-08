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
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace VDS.RDF
{
    /// <summary>
    /// Static Helper Class for making HTTP Requests
    /// </summary>
    /// <remarks>
    /// <para>
    /// Added in order to get the library to run on Silverlight and other platforms which forbid sychronous requests - not yet implemented or used
    /// </para>
    /// </remarks>
    public static class HttpHelper
    {

        public static HttpWebResponse GetResponse(HttpWebRequest request)
        {
#if SILVERLIGHT
            throw new NotImplementedException();
#else
            return (HttpWebResponse)request.GetResponse();
#endif
        }

        public static Stream GetResponse(HttpWebResponse response)
        {
#if SILVERLIGHT
            throw new NotImplementedException();
#else
            return response.GetResponseStream();
#endif
        }
    }
}
