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
using System.Linq;
using System.Text;
using VDS.RDF.Graphs;
using VDS.RDF.Nodes;
using VDS.RDF.Query;

namespace VDS.RDF.Parsing.Handlers
{
    /// <summary>
    /// Abstract Base Class for Handlers
    /// </summary>
    public abstract class BaseHandler
        : INodeFactory
    {
        private INodeFactory _factory;

        /// <summary>
        /// Creates a new Handler
        /// </summary>
        protected BaseHandler()
            : this(new NodeFactory()) { }

        /// <summary>
        /// Creates a new Handler using the given Node Factory
        /// </summary>
        /// <param name="factory">Node Factory</param>
        protected BaseHandler(INodeFactory factory)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            this._factory = factory;
        }

        /// <summary>
        /// Gets/Sets the in-use Node Factory
        /// </summary>
        protected INodeFactory NodeFactory
        {
            get
            {
                return this._factory;
            }
            set
            {
                if (value == null) throw new InvalidOperationException("Cannot set the NodeFactory of a Handler to be null");
                this._factory = value;
            }
        }

        #region INodeFactory Members

        /// <summary>
        /// Creates a Blank Node
        /// </summary>
        /// <returns></returns>
        public virtual INode CreateBlankNode()
        {
            return this._factory.CreateBlankNode();
        }

        /// <summary>
        /// Creates a blank node
        /// </summary>
        /// <param name="id">ID</param>
        /// <returns></returns>
        public virtual INode CreateBlankNode(Guid id)
        {
            return this._factory.CreateBlankNode(id);
        }

        /// <summary>
        /// Creates a Graph Literal Node
        /// </summary>
        /// <returns></returns>
        public virtual INode CreateGraphLiteralNode()
        {
            return this._factory.CreateGraphLiteralNode();
        }

        /// <summary>
        /// Creates a Graph Literal Node with the given sub-graph
        /// </summary>
        /// <param name="subgraph">Sub-graph</param>
        /// <returns></returns>
        public virtual INode CreateGraphLiteralNode(IGraph subgraph)
        {
            return this._factory.CreateGraphLiteralNode(subgraph);
        }

        /// <summary>
        /// Creates a Literal Node with the given Datatype
        /// </summary>
        /// <param name="literal">Value</param>
        /// <param name="datatype">Datatype URI</param>
        /// <returns></returns>
        public virtual INode CreateLiteralNode(string literal, Uri datatype)
        {
            return this._factory.CreateLiteralNode(literal, datatype);
        }

        /// <summary>
        /// Creates a Literal Node
        /// </summary>
        /// <param name="literal">Value</param>
        /// <returns></returns>
        public virtual INode CreateLiteralNode(string literal)
        {
            return this._factory.CreateLiteralNode(literal);
        }

        /// <summary>
        /// Creates a Literal Node with the given Language
        /// </summary>
        /// <param name="literal">Value</param>
        /// <param name="langspec">Language</param>
        /// <returns></returns>
        public virtual INode CreateLiteralNode(string literal, string langspec)
        {
            return this._factory.CreateLiteralNode(literal, langspec);
        }

        /// <summary>
        /// Creates a URI Node
        /// </summary>
        /// <param name="uri">URI</param>
        /// <returns></returns>
        public virtual INode CreateUriNode(Uri uri)
        {
            return this._factory.CreateUriNode(uri);
        }

        /// <summary>
        /// Creates a Variable Node
        /// </summary>
        /// <param name="varname">Variable Name</param>
        /// <returns></returns>
        public virtual INode CreateVariableNode(string varname)
        {
            return this._factory.CreateVariableNode(varname);
        }

        #endregion
    }

    /// <summary>
    /// Abstract Base Class for RDF Handlers
    /// </summary>
    public abstract class BaseRdfHandler 
        : BaseHandler, IRdfHandler
    {
        private bool _inUse = false;

        /// <summary>
        /// Creates a new RDF Handler
        /// </summary>
        public BaseRdfHandler()
            : this(new NodeFactory()) { }

        /// <summary>
        /// Creates a new RDF Handler using the given Node Factory
        /// </summary>
        /// <param name="factory">Node Factory</param>
        public BaseRdfHandler(INodeFactory factory)
            : base(factory) { }

        #region IRdfHandler Members

        /// <summary>
        /// Starts the Handling of RDF
        /// </summary>
        public void StartRdf()
        {
            if (this._inUse) throw new RdfParseException("Cannot use this Handler as an RDF Handler for parsing as it is already in-use");
            this.StartRdfInternal();
            this._inUse = true;
        }

        /// <summary>
        /// Optionally used by derived Handlers to do additional actions on starting RDF handling
        /// </summary>
        protected virtual void StartRdfInternal()
        { }

        /// <summary>
        /// Ends the Handling of RDF
        /// </summary>
        /// <param name="ok">Whether the parsing completed without error</param>
        public void EndRdf(bool ok)
        {
            if (!this._inUse) throw new RdfParseException("Cannot End RDF Handling as this RDF Handler is not currently in-use");
            this.EndRdfInternal(ok);
            this._inUse = false;
        }

        /// <summary>
        /// Optionally used by derived Handlers to do additional actions on ending RDF handling
        /// </summary>
        /// <param name="ok">Whether the parsing completed without error</param>
        protected virtual void EndRdfInternal(bool ok)
        { }

        /// <summary>
        /// Handles Namespace declarations
        /// </summary>
        /// <param name="prefix">Prefix</param>
        /// <param name="namespaceUri">Namespace URI</param>
        /// <returns></returns>
        public bool HandleNamespace(string prefix, Uri namespaceUri)
        {
            if (!this._inUse) throw new RdfParseException("Cannot Handle Namespace as this RDF Handler is not currently in-use");

            return this.HandleNamespaceInternal(prefix, namespaceUri);
        }

        /// <summary>
        /// Optionally used by derived Handlers to do additional actions on handling namespace declarations
        /// </summary>
        /// <param name="prefix">Prefix</param>
        /// <param name="namespaceUri">Namespace URI</param>
        /// <returns></returns>
        protected virtual bool HandleNamespaceInternal(String prefix, Uri namespaceUri)
        {
            return true;
        }

        /// <summary>
        /// Handles Base URI declarations
        /// </summary>
        /// <param name="baseUri">Base URI</param>
        /// <returns></returns>
        public bool HandleBaseUri(Uri baseUri)
        {
            if (!this._inUse) throw new RdfParseException("Cannot Handle Base URI as this RDF Handler is not currently in-use");

            return this.HandleBaseUriInternal(baseUri);
        }

        /// <summary>
        /// Optionally used by derived Handlers to do additional actions on handling Base URI declarations
        /// </summary>
        /// <param name="baseUri">Base URI</param>
        /// <returns></returns>
        protected virtual bool HandleBaseUriInternal(Uri baseUri)
        {
            return true;
        }

        /// <summary>
        /// Handles Triples
        /// </summary>
        /// <param name="t">Triple</param>
        /// <returns></returns>
        public bool HandleTriple(Triple t)
        {
            if (!this._inUse) throw new RdfParseException("Cannot Handle Triple as this RDF Handler is not currently in-use, please ensure you call the StartRdf() method prior to calling this method");

            return this.HandleTripleInternal(t);
        }

        /// <summary>
        /// Handles Quads
        /// </summary>
        /// <param name="q">Quad</param>
        /// <returns></returns>
        public bool HandleQuad(Quad q)
        {
            if (!this._inUse) throw new RdfParseException("Cannot Handle Quad as this RDF Handler is not currently in-use, please ensure you call the StartRdf() method prior to calling this method");

            return this.HandleQuadInternal(q);
        }

        /// <summary>
        /// Must be overridden by derived handlers to take appropriate Triple handling action
        /// </summary>
        /// <param name="t">Triple</param>
        /// <returns></returns>
        protected abstract bool HandleTripleInternal(Triple t);

        /// <summary>
        /// Must be overridden by derived handlers to take appropriate Quad handling action
        /// </summary>
        /// <param name="q">Quad</param>
        /// <returns></returns>
        protected abstract bool HandleQuadInternal(Quad q);

        /// <summary>
        /// Gets whether the Handler will accept all Triples/Quads i.e. it will never abort handling early
        /// </summary>
        public abstract bool AcceptsAll
        {
            get;
        }

        #endregion
    }
}
