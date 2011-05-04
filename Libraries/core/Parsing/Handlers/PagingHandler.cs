using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Parsing.Handlers
{
    /// <summary>
    /// A RDF Handler which wraps another Handler passing only the Triples falling within a given Limit and Offset to the underlying Handler
    /// </summary>
    public class PagingHandler : BaseRdfHandler, IWrappingRdfHandler
    {
        private IRdfHandler _handler;
        private int _limit = 0, _offset = 0;
        private int _counter = 0;

        public PagingHandler(IRdfHandler handler, int limit, int offset)
            : base(handler)
        {
            if (handler == null) throw new ArgumentNullException("handler");
            this._handler = handler;
            this._limit = Math.Max(-1, limit);
            this._offset = Math.Max(0, offset);
        }

        public PagingHandler(IRdfHandler handler, int limit)
            : this(handler, limit, 0) { }

        public IEnumerable<IRdfHandler> InnerHandlers
        {
            get
            {
                return this._handler.AsEnumerable();
            }
        }

        protected override void StartRdfInternal()
        {
            this._handler.StartRdf();
            this._counter = 0;
        }

        protected override void EndRdfInternal(bool ok)
        {
            this._handler.EndRdf(ok);
            this._counter = 0;
        }

        protected override bool HandleTripleInternal(Triple t)
        {
            //If the Limit is zero stop parsing immediately
            if (this._limit == 0) return false;

            this._counter++;
            if (this._limit > 0)
            {
                //Limit greater than zero means get a maximum of limit triples after the offset
                if (this._counter > this._offset && this._counter <= this._limit + this._offset)
                {
                    return this._handler.HandleTriple(t);
                }
                else if (this._counter > this._limit + this._offset)
                {
                    //Stop parsing when we've reached the limit
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                //Limit less than zero means get all triples after the offset
                if (this._counter > this._offset)
                {
                    return this._handler.HandleTriple(t);
                }
                else
                {
                    return true;
                }
            }
        }

        protected override bool HandleNamespaceInternal(string prefix, Uri namespaceUri)
        {
            return this._handler.HandleNamespace(prefix, namespaceUri);
        }

        protected override bool HandleBaseUriInternal(Uri baseUri)
        {
            return this._handler.HandleBaseUri(baseUri);
        }

        public override bool AcceptsAll
        {
            get 
            {
                return this._limit < 0;
            }
        }
    }
}
