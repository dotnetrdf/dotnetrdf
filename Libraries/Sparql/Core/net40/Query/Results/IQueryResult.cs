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

using VDS.RDF.Graphs;

namespace VDS.RDF.Query.Results
{
    /// <summary>
    /// Represents the result of a SPARQL Query
    /// </summary>
    public interface IQueryResult
    {
        /// <summary>
        /// Gets whether this result represents a tabular result
        /// </summary>
        bool IsTabular { get; }

        /// <summary>
        /// Gets whether this result represents a graph result
        /// </summary>
        bool IsGraph { get; }

        /// <summary>
        /// Gets whether this result represents a boolean result
        /// </summary>
        bool IsBoolean { get; }

        /// <summary>
        /// Gets the tabular results (if any)
        /// </summary>
        ITabularResults Table { get; }

        /// <summary>
        /// Gets the graph result (if any)
        /// </summary>
        IGraph Graph { get; }

        /// <summary>
        /// Gets the boolean result (if any)
        /// </summary>
        bool? Boolean { get; }
    }
}
