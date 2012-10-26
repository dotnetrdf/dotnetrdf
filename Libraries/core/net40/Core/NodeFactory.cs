/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using VDS.Common;

namespace VDS.RDF
{
    /// <summary>
    /// A default implementation of a Node Factory
    /// </summary>
    public class NodeFactory
        : INodeFactory
    {
        protected readonly MultiDictionary<String, Guid> _bnodes = new MultiDictionary<string, Guid>();
        protected readonly Guid _factoryID = Guid.NewGuid();

        /// <summary>
        /// Creates a new Node Factory
        /// </summary>
        public NodeFactory()
        { }

        /// <summary>
        /// Creates a new Node Factory
        /// </summary>
        /// <param name="factoryID">Factory ID</param>
        public NodeFactory(Guid factoryID)
        {
            if (factoryID == null) throw new ArgumentNullException("factoryID");
            this._factoryID = factoryID;
        }

        #region INodeFactory Members

        public Guid FactoryID
        {
            get
            {
                return _factoryID;
            }
        }

        /// <summary>
        /// Creates a Blank Node with a new automatically generated ID
        /// </summary>
        /// <returns></returns>
        public virtual IBlankNode CreateBlankNode()
        {
            return new BlankNode(Guid.NewGuid(), this._factoryID);
        }

        /// <summary>
        /// Creates a Blank Node with the given Node ID
        /// </summary>
        /// <param name="nodeId">Node ID</param>
        /// <returns></returns>
        public IBlankNode CreateBlankNode(string nodeId)
        {
            Guid id;
            if (this._bnodes.TryGetValue(nodeId, out id))
            {
                return new BlankNode(id, this._factoryID);
            }
            else
            {
                id = Guid.NewGuid();
                this._bnodes.Add(nodeId, id);
                return new BlankNode(id, this._factoryID);
            }
        }

        /// <summary>
        /// Creates a Graph Literal Node which represents the empty Subgraph
        /// </summary>
        /// <returns></returns>
        public IGraphLiteralNode CreateGraphLiteralNode()
        {
            return new GraphLiteralNode(null);
        }

        /// <summary>
        /// Creates a Graph Literal Node which represents the given Subgraph
        /// </summary>
        /// <param name="subgraph">Subgraph</param>
        /// <returns></returns>
        public IGraphLiteralNode CreateGraphLiteralNode(IGraph subgraph)
        {
            return new GraphLiteralNode(subgraph);
        }

        /// <summary>
        /// Creates a Literal Node with the given Value and Data Type
        /// </summary>
        /// <param name="literal">Value of the Literal</param>
        /// <param name="datatype">Data Type URI of the Literal</param>
        /// <returns></returns>
        public ILiteralNode CreateLiteralNode(string literal, Uri datatype)
        {
            return new LiteralNode(literal, datatype);
        }

        /// <summary>
        /// Creates a Literal Node with the given Value
        /// </summary>
        /// <param name="literal">Value of the Literal</param>
        /// <returns></returns>
        public ILiteralNode CreateLiteralNode(string literal)
        {
            return new LiteralNode(literal);
        }

        /// <summary>
        /// Creates a Literal Node with the given Value and Language
        /// </summary>
        /// <param name="literal">Value of the Literal</param>
        /// <param name="langspec">Language Specifier for the Literal</param>
        /// <returns></returns>
        public ILiteralNode CreateLiteralNode(string literal, string langspec)
        {
            return new LiteralNode(literal, langspec);
        }

        /// <summary>
        /// Creates a URI Node for the given URI
        /// </summary>
        /// <param name="uri">URI</param>
        /// <returns></returns>
        public IUriNode CreateUriNode(Uri uri)
        {
            return new UriNode(uri);
        }

        /// <summary>
        /// Creates a Variable Node for the given Variable Name
        /// </summary>
        /// <param name="varname"></param>
        /// <returns></returns>
        public IVariableNode CreateVariableNode(string varname)
        {
            return new VariableNode(varname);
        }

        /// <summary>
        /// Creates a new unused Blank Node ID and returns it
        /// </summary>
        /// <returns></returns>
        [Obsolete("Obsolete, use GetNextAnonID() instead", true)]
        public string GetNextBlankNodeID()
        {
            throw new NotSupportedException();
        }

        #endregion
    }

    /// <summary>
    /// A Graph Factory provides access to consistent Graph References so that Nodes and Triples can be instantiated in the correct Graphs
    /// </summary>
    /// <remarks>
    /// <para>
    /// Primarily designed for internal use in some of our code but may prove useful to other users hence is a public class.  Internally this is just a wrapper around a <see cref="TripleStore">TripleStore</see> instance.
    /// </para>
    /// <para>
    /// The main usage for this class is scenarios where consistent graph references matter such as returning node references from out of memory datasets (like SQL backed ones) particularly with regards to blank nodes since blank node equality is predicated upon Graph reference.
    /// </para>
    /// </remarks>
    public class GraphFactory
    {
        private TripleStore _store = new TripleStore();

        /// <summary>
        /// Gets a Graph Reference for the given Graph URI
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        public IGraph this[Uri graphUri]
        {
            get
            {
                if (this._store.HasGraph(graphUri))
                {
                    return this._store[graphUri];
                }
                else
                {
                    Graph g = new Graph();
                    g.BaseUri = graphUri;
                    this._store.Add(g);
                    return g;
                }
            }
        }

        /// <summary>
        /// Gets a Graph Reference for the given Graph URI
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        /// <remarks>
        /// Synonym for the index access method i.e. factory[graphUri]
        /// </remarks>
        public IGraph GetGraph(Uri graphUri)
        {
            return this[graphUri];
        }

        /// <summary>
        /// Gets a Graph Reference for the given Graph URI and indicates whether this was a new Graph reference
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <param name="created">Indicates whether the returned reference was newly created</param>
        /// <returns></returns>
        public IGraph TryGetGraph(Uri graphUri, out bool created)
        {
            if (this._store.HasGraph(graphUri))
            {
                created = false;
                return this._store[graphUri];
            }
            else
            {
                created = true;
                Graph g = new Graph();
                g.BaseUri = graphUri;
                this._store.Add(g);
                return g;
            }
        }

        /// <summary>
        /// Resets the Factory so any Graphs with contents are emptied
        /// </summary>
        public void Reset()
        {
            foreach (IGraph g in this._store.Graphs)
            {
                g.Clear();
            }
        }
    }

    /// <summary>
    /// A private implementation of a Node Factory which returns mock constants regardless of the inputs
    /// </summary>
    /// <remarks>
    /// <para>
    /// Intended for usage in scenarios where the user of the factory does not care about the values returned, for example it is used internally in the <see cref="CountHandler">CountHandler</see> to speed up processing
    /// </para>
    /// </remarks>
    class MockNodeFactory
        : INodeFactory
    {
        private readonly Guid _factoryId = Guid.NewGuid();
        private readonly IBlankNode _bnode;
        private readonly IGraphLiteralNode _glit = new GraphLiteralNode();
        private readonly ILiteralNode _lit = new LiteralNode("mock");
        private readonly UriNode _uri = new UriNode(UriFactory.Create("dotnetrdf:mock"));
        private readonly IVariableNode _var = new VariableNode("mock");

        public MockNodeFactory()
        {
            this._bnode = new BlankNode(Guid.NewGuid(), this._factoryId);
        }

        public Guid FactoryID
        {
            get
            {
                return this._factoryId;
            }
        }

        public IBlankNode CreateBlankNode()
        {
            return this._bnode;
        }

        public IBlankNode CreateBlankNode(string nodeId)
        {
            return this._bnode;
        }

        public IGraphLiteralNode CreateGraphLiteralNode()
        {
            return this._glit;
        }

        public IGraphLiteralNode CreateGraphLiteralNode(IGraph subgraph)
        {
            return this._glit;
        }

        public ILiteralNode CreateLiteralNode(string literal, Uri datatype)
        {
            return this._lit;
        }

        public ILiteralNode CreateLiteralNode(string literal)
        {
            return this._lit;
        }

        public ILiteralNode CreateLiteralNode(string literal, string langspec)
        {
            return this._lit;
        }

        public IUriNode CreateUriNode(Uri uri)
        {
            return this._uri;
        }

        public IVariableNode CreateVariableNode(string varname)
        {
            return this._var;
        }

        public string GetNextBlankNodeID()
        {
            throw new NotImplementedException("Not needed by the MockNodeFactory");
        }
    }
}
