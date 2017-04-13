/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
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

using VDS.RDF.Web;

namespace VDS.RDF.Update.Protocol
{
    /// <summary>
    /// Interface for SPARQL Graph Store HTTP Protocol for Graph Management processors
    /// </summary>
    public interface ISparqlHttpProtocolProcessor
    {
        /// <summary>
        /// Processes a GET operation which should retrieve a Graph from the Store and return it
        /// </summary>
        /// <param name="context">HTTP Context</param>
        void ProcessGet(IHttpContext context);

        /// <summary>
        /// Processes a POST operation which should add triples to a Graph in the Store
        /// </summary>
        /// <param name="context">HTTP Context</param>
        void ProcessPost(IHttpContext context);

        /// <summary>
        /// Processes a POST operation which adds triples to a new Graph in the Store and returns the URI of the newly created Graph
        /// </summary>
        /// <param name="context">HTTP Context</param>
        /// <remarks>
        /// <para>
        /// This operation allows clients to POST data to an endpoint and have it create a Graph and assign a URI for them.
        /// </para>
        /// </remarks>
        void ProcessPostCreate(IHttpContext context);

        /// <summary>
        /// Processes a PUT operation which should save a Graph to the Store completely replacing any existing Graph with the same URI
        /// </summary>
        /// <param name="context">HTTP Context</param>
        void ProcessPut(IHttpContext context);

        /// <summary>
        /// Processes a DELETE operation which delete a Graph from the Store
        /// </summary>
        /// <param name="context">HTTP Context</param>
        void ProcessDelete(IHttpContext context);

        /// <summary>
        /// Processes a HEAD operation which gets information about a Graph in the Store
        /// </summary>
        /// <param name="context">HTTP Context</param>
        void ProcessHead(IHttpContext context);

        /// <summary>
        /// Processes a PATCH operation which may choose
        /// </summary>
        /// <param name="context"></param>
        void ProcessPatch(IHttpContext context);
    }
}
