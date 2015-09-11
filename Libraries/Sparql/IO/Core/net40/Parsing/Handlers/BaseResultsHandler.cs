using System;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Results;

namespace VDS.RDF.Parsing.Handlers
{
    /// <summary>
    /// Abstract Base Class for SPARQL Results Handlers
    /// </summary>
    public abstract class BaseResultsHandler
        : BaseHandler, ISparqlResultsHandler
    {
        private bool _inUse = false;

        /// <summary>
        /// Creates a new SPARQL Results Handler
        /// </summary>
        /// <param name="factory">Node Factory</param>
        protected BaseResultsHandler(INodeFactory factory)
            : base(factory) { }

        /// <summary>
        /// Creates a new SPARQL Results Handler
        /// </summary>
        protected BaseResultsHandler()
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
        /// Handles a result row
        /// </summary>
        /// <param name="result">Result row</param>
        /// <returns></returns>
        public bool HandleResult(IResultRow result)
        {
            if (!this._inUse) throw new RdfParseException("Cannot Handle a Result as this Handler is not currently in-use");
            return this.HandleResultInternal(result);
        }

        /// <summary>
        /// Must be overridden by derived handlers to appropriately handle variable declarations
        /// </summary>
        /// <param name="var">Variable Name</param>
        /// <returns></returns>
        protected abstract bool HandleVariableInternal(String var);

        /// <summary>
        /// Must be overridden by derived handlers to appropriately handle result rows
        /// </summary>
        /// <param name="row">Result</param>
        /// <returns></returns>
        protected abstract bool HandleResultInternal(IResultRow row);

        #endregion
    }
}