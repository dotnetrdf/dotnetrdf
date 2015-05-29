/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-develop@lists.sf.net)

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

using System.Collections.Generic;
using VDS.RDF.Graphs;

namespace VDS.RDF.Query.Engine.Bgps
{
    /// <summary>
    /// Interface for BGP executors
    /// </summary>
    public interface IBgpExecutor
    {
        /// <summary>
        /// Matches a single triple pattern against the active graph as defined by the given context
        /// </summary>
        /// <param name="t">Triple pattern</param>
        /// <param name="context">Execution Context</param>
        /// <returns>Set for each distinct match</returns>
        /// <remarks>
        /// The active graph may be formed of multiple graphs, please see the remarks on <see cref="IExecutionContext.ActiveGraph"/> to understand how it should be interpreted
        /// </remarks>
        IEnumerable<ISolution> Match(Triple t, IExecutionContext context);

        /// <summary>
        /// Matches a single triple pattern with relevant variables from the given input set substituted into it against the active graph as defined by the given context
        /// </summary>
        /// <param name="t">Triple pattern</param>
        /// <param name="input">Input Set</param>
        /// <param name="context">Execution Context</param>
        /// <returns>Set for each distinct match</returns>
        /// <remarks>
        /// The active graph may be formed of multiple graphs, please see the remarks on <see cref="IExecutionContext.ActiveGraph"/> to understand how it should be interpreted
        /// </remarks>
        IEnumerable<ISolution> Match(Triple t, ISolution input, IExecutionContext context);
    }
}