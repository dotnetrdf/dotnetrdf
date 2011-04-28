using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query;

namespace VDS.RDF.Parsing.Handlers
{
    public abstract class BaseHandler : INodeFactory
    {
        private INodeFactory _factory;

        public BaseHandler()
            : this(new NodeFactory()) { }

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

        public IBlankNode CreateBlankNode()
        {
            return this._factory.CreateBlankNode();
        }

        public IBlankNode CreateBlankNode(string nodeId)
        {
            return this._factory.CreateBlankNode(nodeId);
        }

        public IGraphLiteralNode CreateGraphLiteralNode()
        {
            return this._factory.CreateGraphLiteralNode();
        }

        public IGraphLiteralNode CreateGraphLiteralNode(IGraph subgraph)
        {
            return this._factory.CreateGraphLiteralNode(subgraph);
        }

        public ILiteralNode CreateLiteralNode(string literal, Uri datatype)
        {
            return this._factory.CreateLiteralNode(literal, datatype);
        }

        public ILiteralNode CreateLiteralNode(string literal)
        {
            return this._factory.CreateLiteralNode(literal);
        }

        public ILiteralNode CreateLiteralNode(string literal, string langspec)
        {
            return this._factory.CreateLiteralNode(literal, langspec);
        }

        public IUriNode CreateUriNode(Uri uri)
        {
            return this._factory.CreateUriNode(uri);
        }

        public IVariableNode CreateVariableNode(string varname)
        {
            return this._factory.CreateVariableNode(varname);
        }

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
        private INodeFactory _factory;
        private bool _inUse = false;

        public BaseRdfHandler()
            : this(new NodeFactory()) { }

        public BaseRdfHandler(INodeFactory factory)
            : base(factory) { }

        #region IRdfHandler Members

        public void StartRdf()
        {
            if (this._inUse) throw new RdfParseException("Cannot use this Handler as an RDF Handler for parsing as it is already in-use");
            this.StartRdfInternal();
            this._inUse = true;
        }

        protected virtual void StartRdfInternal()
        { }

        public void EndRdf(bool ok)
        {
            if (!this._inUse) throw new RdfParseException("Cannot End RDF Handling as this RDF Handler is not currently in-use");
            this.EndRdfInternal(ok);
            this._inUse = false;
        }

        protected virtual void EndRdfInternal(bool ok)
        { }

        public bool HandleNamespace(string prefix, Uri namespaceUri)
        {
            if (!this._inUse) throw new RdfParseException("Cannot Handle Namespace as this RDF Handler is not currently in-use");

            return this.HandleNamespaceInternal(prefix, namespaceUri);
        }

        protected virtual bool HandleNamespaceInternal(String prefix, Uri namespaceUri)
        {
            return true;
        }

        public bool HandleBaseUri(Uri baseUri)
        {
            if (!this._inUse) throw new RdfParseException("Cannot Handle Base URI as this RDF Handler is not currently in-use");

            return this.HandleBaseUriInternal(baseUri);
        }

        protected virtual bool HandleBaseUriInternal(Uri baseUri)
        {
            return true;
        }

        public bool HandleTriple(Triple t)
        {
            if (!this._inUse) throw new RdfParseException("Cannot Handle Triple as this RDF Handler is not currently in-use");

            return this.HandleTripleInternal(t);
        }

        protected abstract bool HandleTripleInternal(Triple t);

        public abstract bool AcceptsAll
        {
            get;
        }

        #endregion
    }

    public abstract class BaseResultsHandler : BaseHandler, ISparqlResultsHandler
    {
        private bool _inUse = false;

        public BaseResultsHandler(INodeFactory factory)
            : base(factory) { }

        public BaseResultsHandler()
            : this(new NodeFactory()) { }

        #region ISparqlResultsHandler Members

        public void StartResults()
        {
            if (this._inUse) throw new RdfParseException("Cannot use this Handler as an Results Handler for parsing as it is already in-use");
            this.StartResultsInternal();
            this._inUse = true;
        }

        protected virtual void StartResultsInternal()
        { }

        public void EndResults(bool ok)
        {
            if (!this._inUse) throw new RdfParseException("Cannot End Results Handling as this Results Handler is not currently in-use");
            this.EndResultsInternal(ok);
            this._inUse = false;

        }

        protected virtual void EndResultsInternal(bool ok)
        { }

        public void HandleBooleanResult(bool result)
        {
            if (!this._inUse) throw new RdfParseException("Cannot Handle a Boolean Result as this Handler is not currently in-use");
            this.HandleBooleanResultInternal(result);
        }


        protected abstract void HandleBooleanResultInternal(bool result);

        public bool HandleVariable(String var)
        {
            if (!this._inUse) throw new RdfParseException("Cannot Handle a Variable as this Handler is not currently in-use");
            return this.HandleVariableInternal(var);
        }

        protected abstract bool HandleVariableInternal(String var);

        public bool HandleResult(SparqlResult result)
        {
            if (!this._inUse) throw new RdfParseException("Cannot Handle a Result as this Handler is not currently in-use");
            return this.HandleResultInternal(result);
        }

        protected abstract bool HandleResultInternal(SparqlResult result);

        #endregion
    }
}
