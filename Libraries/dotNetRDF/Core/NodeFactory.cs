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

using System;

namespace VDS.RDF
{
    /// <summary>
    /// A default implementation of a Node Factory which generates Nodes unrelated to Graphs (wherever possible we suggest using a Graph based implementation instead)
    /// </summary>
    public class NodeFactory 
        : INodeFactory
    {
        private BlankNodeMapper _bnodeMap = new BlankNodeMapper();

        /// <summary>
        /// Creates a new Node Factory
        /// </summary>
        public NodeFactory()
        { }

        #region INodeFactory Members

        /// <summary>
        /// Creates a Blank Node with a new automatically generated ID
        /// </summary>
        /// <returns></returns>
        public IBlankNode CreateBlankNode()
        {
            return new BlankNode(this);
        }

        /// <summary>
        /// Creates a Blank Node with the given Node ID
        /// </summary>
        /// <param name="nodeId">Node ID</param>
        /// <returns></returns>
        public IBlankNode CreateBlankNode(string nodeId)
        {
            _bnodeMap.CheckID(ref nodeId);
            return new BlankNode(null, nodeId);
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
            return new GraphLiteralNode(null, subgraph);
        }

        /// <summary>
        /// Creates a Literal Node with the given Value and Data Type
        /// </summary>
        /// <param name="literal">Value of the Literal</param>
        /// <param name="datatype">Data Type URI of the Literal</param>
        /// <returns></returns>
        public ILiteralNode CreateLiteralNode(string literal, Uri datatype)
        {
            return new LiteralNode(null, literal, datatype);
        }

        /// <summary>
        /// Creates a Literal Node with the given Value
        /// </summary>
        /// <param name="literal">Value of the Literal</param>
        /// <returns></returns>
        public ILiteralNode CreateLiteralNode(string literal)
        {
            return new LiteralNode(null, literal);
        }

        /// <summary>
        /// Creates a Literal Node with the given Value and Language
        /// </summary>
        /// <param name="literal">Value of the Literal</param>
        /// <param name="langspec">Language Specifier for the Literal</param>
        /// <returns></returns>
        public ILiteralNode CreateLiteralNode(string literal, string langspec)
        {
            return new LiteralNode(null, literal, langspec);
        }

        /// <summary>
        /// Creates a URI Node for the given URI
        /// </summary>
        /// <param name="uri">URI</param>
        /// <returns></returns>
        public IUriNode CreateUriNode(Uri uri)
        {
            return new UriNode(null, uri);
        }

        /// <summary>
        /// Creates a Variable Node for the given Variable Name
        /// </summary>
        /// <param name="varname"></param>
        /// <returns></returns>
        public IVariableNode CreateVariableNode(string varname)
        {
            return new VariableNode(null, varname);
        }

        /// <summary>
        /// Creates a new unused Blank Node ID and returns it
        /// </summary>
        /// <returns></returns>
        public string GetNextBlankNodeID()
        {
            return _bnodeMap.GetNextID();
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
                if (_store.HasGraph(graphUri))
                {
                    return _store[graphUri];
                }
                else
                {
                    Graph g = new Graph();
                    g.BaseUri = graphUri;
                    _store.Add(g);
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
            if (_store.HasGraph(graphUri))
            {
                created = false;
                return _store[graphUri];
            }
            else
            {
                created = true;
                Graph g = new Graph();
                g.BaseUri = graphUri;
                _store.Add(g);
                return g;
            }
        }

        /// <summary>
        /// Resets the Factory so any Graphs with contents are emptied
        /// </summary>
        public void Reset()
        {
            foreach (IGraph g in _store.Graphs)
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
    /// Intended for usage in scenarios where the user of the factory does not care about the values returned, for example it is used internally in the <see cref="VDS.RDF.Parsing.Handlers.CountHandler">CountHandler</see> to speed up processing
    /// </para>
    /// </remarks>
    class MockNodeFactory
        : INodeFactory
    {
        private IBlankNode _bnode = new BlankNode(null, "mock");
        private IGraphLiteralNode _glit = new GraphLiteralNode(null);
        private ILiteralNode _lit = new LiteralNode(null, "mock");
        private IUriNode _uri = new UriNode(null, UriFactory.Create("dotnetrdf:mock"));
        private IVariableNode _var = new VariableNode(null, "mock");

        #region INodeFactory Members

        public IBlankNode CreateBlankNode()
        {
            return _bnode;
        }

        public IBlankNode CreateBlankNode(string nodeId)
        {
            return _bnode;
        }

        public IGraphLiteralNode CreateGraphLiteralNode()
        {
            return _glit;
        }

        public IGraphLiteralNode CreateGraphLiteralNode(IGraph subgraph)
        {
            return _glit;
        }

        public ILiteralNode CreateLiteralNode(string literal, Uri datatype)
        {
            return _lit;
        }

        public ILiteralNode CreateLiteralNode(string literal)
        {
            return _lit;
        }

        public ILiteralNode CreateLiteralNode(string literal, string langspec)
        {
            return _lit;
        }

        public IUriNode CreateUriNode(Uri uri)
        {
            return _uri;
        }

        public IVariableNode CreateVariableNode(string varname)
        {
            return _var;
        }

        public string GetNextBlankNodeID()
        {
            throw new NotImplementedException("Not needed by the MockNodeFactory");
        }

        #endregion
    }
}
