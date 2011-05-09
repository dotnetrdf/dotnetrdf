using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query;

namespace VDS.RDF.Parsing.Handlers
{
    /// <summary>
    /// Abstract Base Class for Handlers
    /// </summary>
    public abstract class BaseHandler : INodeFactory
    {
        private INodeFactory _factory;

        /// <summary>
        /// Creates a new Handler
        /// </summary>
        public BaseHandler()
            : this(new NodeFactory()) { }

        /// <summary>
        /// Creates a new Handler using the given Node Factory
        /// </summary>
        /// <param name="factory">Node Factory</param>
        public BaseHandler(INodeFactory factory)
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
        public IBlankNode CreateBlankNode()
        {
            return this._factory.CreateBlankNode();
        }

        /// <summary>
        /// Creates a Blank Node with the given ID
        /// </summary>
        /// <param name="nodeId">Node ID</param>
        /// <returns></returns>
        public IBlankNode CreateBlankNode(string nodeId)
        {
            return this._factory.CreateBlankNode(nodeId);
        }

        /// <summary>
        /// Creates a Graph Literal Node
        /// </summary>
        /// <returns></returns>
        public IGraphLiteralNode CreateGraphLiteralNode()
        {
            return this._factory.CreateGraphLiteralNode();
        }

        /// <summary>
        /// Creates a Graph Literal Node with the given sub-graph
        /// </summary>
        /// <param name="subgraph">Sub-graph</param>
        /// <returns></returns>
        public IGraphLiteralNode CreateGraphLiteralNode(IGraph subgraph)
        {
            return this._factory.CreateGraphLiteralNode(subgraph);
        }

        /// <summary>
        /// Creates a Literal Node with the given Datatype
        /// </summary>
        /// <param name="literal">Value</param>
        /// <param name="datatype">Datatype URI</param>
        /// <returns></returns>
        public ILiteralNode CreateLiteralNode(string literal, Uri datatype)
        {
            return this._factory.CreateLiteralNode(literal, datatype);
        }

        /// <summary>
        /// Creates a Literal Node
        /// </summary>
        /// <param name="literal">Value</param>
        /// <returns></returns>
        public ILiteralNode CreateLiteralNode(string literal)
        {
            return this._factory.CreateLiteralNode(literal);
        }

        /// <summary>
        /// Creates a Literal Node with the given Language
        /// </summary>
        /// <param name="literal">Value</param>
        /// <param name="langspec">Language</param>
        /// <returns></returns>
        public ILiteralNode CreateLiteralNode(string literal, string langspec)
        {
            return this._factory.CreateLiteralNode(literal, langspec);
        }

        /// <summary>
        /// Creates a URI Node
        /// </summary>
        /// <param name="uri">URI</param>
        /// <returns></returns>
        public IUriNode CreateUriNode(Uri uri)
        {
            return this._factory.CreateUriNode(uri);
        }

        /// <summary>
        /// Creates a Variable Node
        /// </summary>
        /// <param name="varname">Variable Name</param>
        /// <returns></returns>
        public IVariableNode CreateVariableNode(string varname)
        {
            return this._factory.CreateVariableNode(varname);
        }

        /// <summary>
        /// Gets the next available Blank Node ID
        /// </summary>
        /// <returns></returns>
        public string GetNextBlankNodeID()
        {
            return this._factory.GetNextBlankNodeID();
        }

        #endregion
    }

    /// <summary>
    /// Abstract Base Class for RDF Handlers
    /// </summary>
    public abstract class BaseRdfHandler : BaseHandler, IRdfHandler
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
            if (!this._inUse) throw new RdfParseException("Cannot Handle Triple as this RDF Handler is not currently in-use");

            return this.HandleTripleInternal(t);
        }

        /// <summary>
        /// Must be overridden by derived handlers to take appropriate Triple handling action
        /// </summary>
        /// <param name="t">Triple</param>
        /// <returns></returns>
        protected abstract bool HandleTripleInternal(Triple t);

        /// <summary>
        /// Gets whether the Handler will accept all Triples i.e. it will never abort handling early
        /// </summary>
        public abstract bool AcceptsAll
        {
            get;
        }

        #endregion
    }

    /// <summary>
    /// Abstract Base Class for SPARQL Results Handlers
    /// </summary>
    public abstract class BaseResultsHandler : BaseHandler, ISparqlResultsHandler
    {
        private bool _inUse = false;

        /// <summary>
        /// Creates a new SPARQL Results Handler
        /// </summary>
        /// <param name="factory">Node Factory</param>
        public BaseResultsHandler(INodeFactory factory)
            : base(factory) { }

        /// <summary>
        /// Creates a new SPARQL Results Handler
        /// </summary>
        public BaseResultsHandler()
            : this(new NodeFactory()) { }

        #region ISparqlResultsHandler Members

        /// <summary>
        /// Starts Results Handling
        /// </summary>
        public void StartResults()
        {
            if (this._inUse) throw new RdfParseException("Cannot use this Handler as an Results Handler for parsing as it is already in-use");
            this.StartResultsInternal();
            this._inUse = true;
        }

        /// <summary>
        /// Optionally used by derived classes to take additional actions on starting Results Handling
        /// </summary>
        protected virtual void StartResultsInternal()
        { }

        /// <summary>
        /// Ends Results Handling
        /// </summary>
        /// <param name="ok">Whether parsing completed without error</param>
        public void EndResults(bool ok)
        {
            if (!this._inUse) throw new RdfParseException("Cannot End Results Handling as this Results Handler is not currently in-use");
            this.EndResultsInternal(ok);
            this._inUse = false;

        }

        /// <summary>
        /// Optionally used by derived classes to take additional actions on ending Results Handling
        /// </summary>
        /// <param name="ok">Whether parsing completed without error</param>
        protected virtual void EndResultsInternal(bool ok)
        { }

        /// <summary>
        /// Handles a Boolean Results
        /// </summary>
        /// <param name="result">Result</param>
        public void HandleBooleanResult(bool result)
        {
            if (!this._inUse) throw new RdfParseException("Cannot Handle a Boolean Result as this Handler is not currently in-use");
            this.HandleBooleanResultInternal(result);
        }

        /// <summary>
        /// Must be overridden by derived handlers to appropriately handle boolean results
        /// </summary>
        /// <param name="result">Result</param>
        protected abstract void HandleBooleanResultInternal(bool result);

        /// <summary>
        /// Handles a Variable declaration
        /// </summary>
        /// <param name="var">Variable Name</param>
        /// <returns></returns>
        public bool HandleVariable(String var)
        {
            if (!this._inUse) throw new RdfParseException("Cannot Handle a Variable as this Handler is not currently in-use");
            return this.HandleVariableInternal(var);
        }

        /// <summary>
        /// Must be overridden by derived handlers to appropriately handle variable declarations
        /// </summary>
        /// <param name="var">Variable Name</param>
        /// <returns></returns>
        protected abstract bool HandleVariableInternal(String var);

        /// <summary>
        /// Handlers SPARQL Results
        /// </summary>
        /// <param name="result">Result</param>
        /// <returns></returns>
        public bool HandleResult(SparqlResult result)
        {
            if (!this._inUse) throw new RdfParseException("Cannot Handle a Result as this Handler is not currently in-use");
            return this.HandleResultInternal(result);
        }

        /// <summary>
        /// Must be overridden by derived handlers to appropriately handler SPARQL Results
        /// </summary>
        /// <param name="result">Result</param>
        /// <returns></returns>
        protected abstract bool HandleResultInternal(SparqlResult result);

        #endregion
    }
}
