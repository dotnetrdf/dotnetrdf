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
using VDS.RDF.Query.Results;

namespace VDS.RDF.Query.Processors
{
    /// <summary>
    /// Interface for SPARQL Query Processors
    /// </summary>
    /// <remarks>
    /// <para>
    /// A SPARQL Query Processor is a class that knows how to evaluate SPARQL queries against some data source to which the processor has access
    /// </para>
    /// <para>
    /// The point of this interface is to allow for end users to implement custom query processors or to extend and modify the behaviour of the default Leviathan engine as required.
    /// </para>
    /// </remarks>
    public interface IQueryProcessor
    {
        /// <summary>
        /// Executes the given query
        /// </summary>
        /// <param name="query">Query</param>
        /// <returns>Query Result</returns>
        IQueryResult Execute(IQuery query);

        /// <summary>
        /// Executes the given query asynchronously
        /// </summary>
        /// <param name="query">Query</param>
        /// <param name="callback">Callback for when the query completes</param>
        /// <param name="state">State to be passed to the callback</param>
        void Execute(IQuery query, QueryCallback callback, Object state);
    }
}
