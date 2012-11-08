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
using VDS.Web.Handlers;

namespace VDS.Web.Modules
{
    /// <summary>
    /// Interface for Modules which can be used to modify the request/response as desired
    /// </summary>
    /// <remarks>
    /// <para>
    /// Modules are either applied Pre-Request (i.e. before a <see cref="IHttpListenerHandler">IHttpListenerHandler</see> is selected to provide the actual response) or Pre-Response (i.e. before the response object is closed and sent to the client)
    /// </para>
    /// </remarks>
    public interface IHttpListenerModule
    {
        /// <summary>
        /// Processes a Request and returns false if no further handling of the request should take place
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// If used as a Pre-Request Module returning false will cause the actual process request step where a <see cref="IHttpListenerHandler">IHttpListenerHandler</see> is applied to be skipped as well as any further pre-request modules
        /// </para>
        /// <para>
        /// If used as a Pre-Response Module returning false will cause any further Pre-Response modules to be skipped
        /// </para>
        /// </remarks>
        bool ProcessRequest(HttpServerContext context);
    }
}
