/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2020 dotNetRDF Project (http://dotnetrdf.org/)
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
    /// A default implementation of a Node Factory which generates Nodes unrelated to Graphs (wherever possible we suggest using a Graph based implementation instead).
    /// </summary>
    public class NodeFactory 
        : INodeFactory
    {
        private readonly BlankNodeMapper _bnodeMap = new BlankNodeMapper();

        /// <summary>
        /// Get the namespace map for this node factory.
        /// </summary>
        public INamespaceMapper NamespaceMap { get; }

        /// <summary>
        /// Get or set the base URI used to resolve relative URI references.
        /// </summary>
        /// <remarks>If <see cref="BaseUri"/> is null, attempting to invoke <see cref="CreateUriNode(Uri)"/> with a relative URI or <see cref="CreateUriNode(string)"/> with a QName that resolves to a relative URI will result in a <see cref="RdfException"/> being raised.</remarks>
        public Uri BaseUri { get; set; }

       
        /// <summary>
        /// Creates a new Node Factory.
        /// </summary>
        /// <param name="baseUri">The initial base URI to use for the resolution of relative URI references. Defaults to null.</param>
        /// <param name="namespaceMap">The namespace map to use for the resolution of QNames. If not specified, a default <see cref="NamespaceMapper"/> instance will be created.</param>
        /// <param name="normalizeLiteralValues">Whether or not to normalize the value strings of literal nodes.</param>
        public NodeFactory(Uri baseUri = null, INamespaceMapper namespaceMap = null, bool normalizeLiteralValues = false)
        {
            BaseUri = baseUri;
            NamespaceMap = namespaceMap ?? new NamespaceMapper();
            NormalizeLiteralValues = normalizeLiteralValues;
        }

        #region INodeFactory Members

        /// <inheritdoc />
#pragma warning disable CS0618 // Type or member is obsolete
        public bool NormalizeLiteralValues { get; set; } = Options.LiteralValueNormalization;

        /// <summary>
        /// Resolve a QName to a URI using this factory's <see cref="INodeFactory.NamespaceMap"/> and <see cref="INodeFactory.BaseUri"/>.
        /// </summary>
        /// <param name="qName"></param>
        /// <returns></returns>
        public Uri ResolveQName(string qName)
        {
            return UriFactory.Create(Tools.ResolveQName(qName, NamespaceMap, BaseUri));
        }
#pragma warning restore CS0618 // Type or member is obsolete

        /// <summary>
        /// Creates a Blank Node with a new automatically generated ID.
        /// </summary>
        /// <returns></returns>
        public IBlankNode CreateBlankNode()
        {
            return new BlankNode(this);
        }

        /// <summary>
        /// Creates a Blank Node with the given Node ID.
        /// </summary>
        /// <param name="nodeId">Node ID.</param>
        /// <returns></returns>
        public IBlankNode CreateBlankNode(string nodeId)
        {
            _bnodeMap.CheckID(ref nodeId);
            return new BlankNode(nodeId);
        }

        /// <summary>
        /// Creates a Graph Literal Node which represents the empty Sub-graph.
        /// </summary>
        /// <returns></returns>
        public IGraphLiteralNode CreateGraphLiteralNode()
        {
            return new GraphLiteralNode();
        }

        /// <summary>
        /// Creates a Graph Literal Node which represents the given Sub-graph.
        /// </summary>
        /// <param name="subgraph">Subgraph.</param>
        /// <returns></returns>
        public IGraphLiteralNode CreateGraphLiteralNode(IGraph subgraph)
        {
            return new GraphLiteralNode(subgraph);
        }

        /// <summary>
        /// Creates a Literal Node with the given Value and Data Type.
        /// </summary>
        /// <param name="literal">Value of the Literal.</param>
        /// <param name="datatype">Data Type URI of the Literal.</param>
        /// <returns></returns>
        public ILiteralNode CreateLiteralNode(string literal, Uri datatype)
        {
            return new LiteralNode(literal, datatype, NormalizeLiteralValues);
        }

        /// <summary>
        /// Creates a Literal Node with the given Value.
        /// </summary>
        /// <param name="literal">Value of the Literal.</param>
        /// <returns></returns>
        public ILiteralNode CreateLiteralNode(string literal)
        {
            return new LiteralNode(literal, NormalizeLiteralValues);
        }

        /// <summary>
        /// Creates a Literal Node with the given Value and Language.
        /// </summary>
        /// <param name="literal">Value of the Literal.</param>
        /// <param name="langSpec">Language Specifier for the Literal.</param>
        /// <returns></returns>
        public ILiteralNode CreateLiteralNode(string literal, string langSpec)
        {
            return new LiteralNode(literal, langSpec, NormalizeLiteralValues);
        }

        /// <summary>
        /// Creates a URI Node for the given URI.
        /// </summary>
        /// <param name="uri">URI.</param>
        /// <returns></returns>
        public IUriNode CreateUriNode(Uri uri)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri), "URI must not be null.");
            if (!uri.IsAbsoluteUri)
            {
                if (BaseUri == null)
                {
                    throw new ArgumentException(
                        "Cannot create a URI node from a relative URI as there is no base URI currently defined.");
                }

                uri = new Uri(BaseUri, uri);
            }
            return new UriNode(uri);
        }

        /// <summary>
        /// Creates a URI Node for the given QName using the Graphs NamespaceMap to resolve the QName.
        /// </summary>
        /// <param name="qName">QName.</param>
        /// <returns>A new URI node.</returns>
        public IUriNode CreateUriNode(string qName)
        {
            if (qName == null) throw new ArgumentNullException(nameof(qName), "QName must not be null");
            return CreateUriNode(UriFactory.Create(Tools.ResolveQName(qName, NamespaceMap, BaseUri)));
        }

        /// <summary>
        /// Creates a URI Node that corresponds to the current Base URI of the node factory.
        /// </summary>
        /// <returns>A new URI Node.</returns>
        /// <exception cref="RdfException">Raised if <see cref="BaseUri"/> is currently null.</exception>
        public IUriNode CreateUriNode()
        {
            if (BaseUri == null)
            {
                throw new RdfException(
                    "Cannot create a URI node from the factory base URI because the base URI is not set.");
            }

            return CreateUriNode(BaseUri);
        }

        /// <summary>
        /// Creates a Variable Node for the given Variable Name.
        /// </summary>
        /// <param name="varName"></param>
        /// <returns></returns>
        public IVariableNode CreateVariableNode(string varName)
        {
            return new VariableNode(varName);
        }

        /// <summary>
        /// Creates a new unused Blank Node ID and returns it.
        /// </summary>
        /// <returns></returns>
        public string GetNextBlankNodeID()
        {
            return _bnodeMap.GetNextID();
        }

        #endregion
    }

    /// <summary>
    /// A Graph Factory provides access to consistent Graph References so that Nodes and Triples can be instantiated in the correct Graphs.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Primarily designed for internal use in some of our code but may prove useful to other users hence is a public class.  Internally this is just a wrapper around a <see cref="TripleStore">TripleStore</see> instance.
    /// </para>
    /// <para>
    /// The main usage for this class is scenarios where consistent graph references matter such as returning node references from out of memory datasets (like SQL backed ones) particularly with regards to blank nodes since blank node equality is predicated upon Graph reference.
    /// </para>
    /// </remarks>
    [Obsolete("This class is obsolete and will be removed in a future release. There is no replacement for this class.")]
    public class GraphFactory
    {
        private TripleStore _store = new TripleStore();

        /// <summary>
        /// Gets a Graph Reference for the given Graph URI.
        /// </summary>
        /// <param name="graphUri">Graph URI.</param>
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
                    var g = new Graph();
                    g.BaseUri = graphUri;
                    _store.Add(g);
                    return g;
                }
            }
        }

        /// <summary>
        /// Gets a Graph Reference for the given Graph URI.
        /// </summary>
        /// <param name="graphUri">Graph URI.</param>
        /// <returns></returns>
        /// <remarks>
        /// Synonym for the index access method i.e. factory[graphUri].
        /// </remarks>
        public IGraph GetGraph(Uri graphUri)
        {
            return this[graphUri];
        }

        /// <summary>
        /// Gets a Graph Reference for the given Graph URI and indicates whether this was a new Graph reference.
        /// </summary>
        /// <param name="graphUri">Graph URI.</param>
        /// <param name="created">Indicates whether the returned reference was newly created.</param>
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
                var g = new Graph();
                g.BaseUri = graphUri;
                _store.Add(g);
                return g;
            }
        }

        /// <summary>
        /// Resets the Factory so any Graphs with contents are emptied.
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
    /// A private implementation of a Node Factory which returns mock constants regardless of the inputs.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Intended for usage in scenarios where the user of the factory does not care about the values returned, for example it is used internally in the <see cref="VDS.RDF.Parsing.Handlers.CountHandler">CountHandler</see> to speed up processing.
    /// </para>
    /// </remarks>
    class MockNodeFactory
        : INodeFactory
    {
        private IBlankNode _bnode = new BlankNode("mock");
        private IGraphLiteralNode _glit = new GraphLiteralNode();
        private ILiteralNode _lit = new LiteralNode("mock", false);
        private IUriNode _uri = new UriNode(UriFactory.Create("dotnetrdf:mock"));
        private IVariableNode _var = new VariableNode("mock");

        #region INodeFactory Members

        public Uri BaseUri { get; set; }

        public INamespaceMapper NamespaceMap { get; } = new NamespaceMapper();

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

        public IUriNode CreateUriNode(string qName)
        {
            return _uri;
        }

        public IUriNode CreateUriNode()
        {
            throw new NotImplementedException();
        }

        public IVariableNode CreateVariableNode(string varname)
        {
            return _var;
        }

        public string GetNextBlankNodeID()
        {
            throw new NotImplementedException("Not needed by the MockNodeFactory");
        }

        public bool NormalizeLiteralValues
        {
            get => false;
            set => throw new NotImplementedException("Not needed by the MockNodeFactory");
        }

        public Uri ResolveQName(string qName)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
