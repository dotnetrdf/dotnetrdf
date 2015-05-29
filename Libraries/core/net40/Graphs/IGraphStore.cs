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
using VDS.RDF.Nodes;

namespace VDS.RDF.Graphs
{
    /// <summary>
    /// Interface for Graph Stores, an extension of an <see cref="IQuadStore"/> that allows the data to be accessed in terms of <see cref="IGraph"/> instances
    /// </summary>
    /// <remarks>
    /// <para>
    /// This interface is a hybrid of the old ITripleStore and ISparqlDataset interfaces from the 1.0 APIs, it allows for data to be manipulated at either the Graph, Triple or Quad level as desired.
    /// </para>
    /// </remarks>
    public interface IGraphStore 
        : IQuadStore
    {
        /// <summary>
        /// Gets the Graphs in the Store
        /// </summary>
        IEnumerable<IGraph> Graphs
        { 
            get;
        }

        /// <summary>
        /// Gets a specific Graph from the Store
        /// </summary>
        /// <param name="graphName">Graph name</param>
        /// <returns>A Graph if it exists in the Store, an error otherwise</returns>
        /// <remarks>
        /// <em>null</em> or <see cref="Quad.DefaultGraphNode"/> may be used to access the default unnamed graph
        /// </remarks>
        IGraph this[INode graphName]
        { 
            get; 
        }

        /// <summary>
        /// Adds the contents of a Graph to the stores default unnamed graph
        /// </summary>
        /// <param name="g">Graph</param>
        void Add(IGraph g);

        /// <summary>
        /// Adds the contents of a Graph to the store using the given graph name for the added Quads
        /// </summary>
        /// <param name="graphName">Name of the Graph to add to</param>
        /// <param name="g">Graph</param>
        /// <remarks>
        /// <em>null</em> or <see cref="Quad.DefaultGraphNode"/> may be used to access the default unnamed graph
        /// </remarks>
        void Add(INode graphName, IGraph g);

        /// <summary>
        /// Copies the contents of one graph to another
        /// </summary>
        /// <param name="srcName">Source Graph name</param>
        /// <param name="destName">Target Graph name</param>
        /// <param name="overwrite">If true the contents of the target graph are overwritten, if false the copied data is added to the existing data in the target graph</param>
        /// <remarks>
        /// <em>null</em> or <see cref="Quad.DefaultGraphNode"/> may be used to access the default unnamed graph
        /// </remarks>
        void Copy(INode srcName, INode destName, bool overwrite);

        /// <summary>
        /// Moves the contents of one graph to another
        /// </summary>
        /// <param name="srcName">Source Graph name</param>
        /// <param name="destName">Destination Graph name</param>
        /// <param name="overwrite">If true the contents of the target graph are overwritten, if false the moved data is added to the existing data in the target graph</param>
        /// <remarks>
        /// <em>null</em> or <see cref="Quad.DefaultGraphNode"/> may be used to access the default unnamed graph
        /// </remarks>
        void Move(INode srcName, INode destName, bool overwrite);

        /// <summary>
        /// Removes the contents of the given graph from the stores default unnamed graph
        /// </summary>
        /// <param name="g">Graph</param>
        void Remove(IGraph g);

        /// <summary>
        /// Removes the contents of the given graph from a specific graph in the store
        /// </summary>
        /// <param name="graphName">Graph name</param>
        /// <param name="g">Graph</param>
        /// <remarks>
        /// <em>null</em> or <see cref="Quad.DefaultGraphNode"/> may be used to access the default unnamed graph
        /// </remarks>
        void Remove(INode graphName, IGraph g);

        /// <summary>
        /// Removes the contents of the graph with the given URI
        /// </summary>
        /// <param name="graphName">Graph name</param>
        /// <remarks>
        /// <em>null</em> or <see cref="Quad.DefaultGraphNode"/> may be used to access the default unnamed graph
        /// </remarks>
        void Remove(INode graphName);
    }
}
