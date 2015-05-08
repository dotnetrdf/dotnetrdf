/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2013 dotNetRDF Project (dotnetrdf-develop@lists.sf.net)

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
using VDS.RDF.Nodes;

namespace VDS.RDF.Graphs
{
    /// <summary>
    /// Interface for Quad Stores
    /// </summary>
    public interface IQuadStore 
    {
        /// <summary>
        /// Adds a Quad to the Store
        /// </summary>
        /// <param name="q">Quad</param>
        void Add(Quad q);

        /// <summary>
        /// Clears all quads with the given graph name
        /// </summary>
        /// <param name="graphName">Graph name</param>
        /// <remarks>
        /// <em>null</em> or <see cref="Quad.DefaultGraphNode"/> may be used to access the default unnamed graph
        /// </remarks>
        void Clear(INode graphName);

        /// <summary>
        /// Removes a Quad from the store
        /// </summary>
        /// <param name="q">Quad</param>
        void Remove(Quad q);

        /// <summary>
        /// Get all Quads in the store
        /// </summary>
        IEnumerable<Quad> Quads { get; }

        /// <summary>
        /// Gets the names of the Graphs in the Store
        /// </summary>
        IEnumerable<INode> GraphNames { get; }

        /// <summary>
        /// Is the given Quad contained in the store?
        /// </summary>
        /// <param name="q">Quad</param>
        /// <returns>True if present, false otherwise</returns>
        bool Contains(Quad q);

        /// <summary>
        /// Find any quads matching the given search criteria with any graph name, null is treated as a wildcard.  Implementations should not retain duplicates though it should be impossible to have duplicate quads in the first place.
        /// </summary>
        /// <param name="s">Subject</param>
        /// <param name="p">Predicate</param>
        /// <param name="o">Object</param>
        /// <returns>Enumerable of quads</returns>
        IEnumerable<Quad> Find(INode s, INode p, INode o);

        /// <summary>
        /// Finds any quads matching the given search criteria, null is treated as a wildcard.  Implementations should not retain duplicates though it should be impossible to have duplicate quads in the first place.
        /// </summary>
        /// <param name="g">Graph name</param>
        /// <param name="s">Subject</param>
        /// <param name="p">Predicate</param>
        /// <param name="o">Object</param>
        /// <returns>Enumerable of quads</returns>
        /// <remarks>
        /// <see cref="Quad.DefaultGraphNode"/> <strong>must</strong> be used to access the default unnamed graph since <em>null</em> is treated as a wildcard for this method
        /// </remarks>
        IEnumerable<Quad> Find(INode g, INode s, INode p, INode o);

        /// <summary>
        /// Determines whether the store has any quads with the specified graph name
        /// </summary>
        /// <param name="graphName">Graph name</param>
        /// <returns>True if the any relevant quads exist in the store, false otherwise</returns>
        /// <remarks>
        /// <em>null</em> or <see cref="Quad.DefaultGraphNode"/> may be used to access the default unnamed graph
        /// </remarks>
        bool HasGraph(INode graphName);
    }
}