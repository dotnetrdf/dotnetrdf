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
using VDS.Web;

namespace VDS.Web.Handlers
{
    /// <summary>
    /// Interface represents a collection of mappings for handlers
    /// </summary>
    public interface IHttpListenerHandlerCollection
        : IEnumerable<HttpRequestMapping>
    {
        /// <summary>
        /// Adds a mapping
        /// </summary>
        /// <param name="mapping">Mapping</param>
        void AddMapping(HttpRequestMapping mapping);

        /// <summary>
        /// Gets the size of the collection
        /// </summary>
        int Count 
        {
            get;
        }

        /// <summary>
        /// Gets a Handler based on the current server context
        /// </summary>
        /// <param name="context">Server Context</param>
        /// <returns></returns>
        IHttpListenerHandler GetHandler(HttpServerContext context);

        /// <summary>
        /// Gets a Handler based on the given type
        /// </summary>
        /// <param name="handlerType">Handler Type</param>
        /// <returns></returns>
        IHttpListenerHandler GetHandler(Type handlerType);

        /// <summary>
        /// Inserts a mapping into the collection
        /// </summary>
        /// <param name="mapping">Mapping</param>
        /// <param name="insertAt">Index to insert at</param>
        void InsertMapping(HttpRequestMapping mapping, int insertAt);

        /// <summary>
        /// Removes a mapping from the collection
        /// </summary>
        /// <param name="mapping">Mapping</param>
        void RemoveMapping(HttpRequestMapping mapping);

        /// <summary>
        /// Removes a mapping at the given index
        /// </summary>
        /// <param name="removeAt">Index to remove at</param>
        void RemoveMapping(int removeAt);
    }
}
