using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Parsing.Handlers
{
    /// <summary>
    /// A RDF Handler which simply counts the Triples
    /// </summary>
    public class CountHandler : BaseRdfHandler
    {
        private int _counter = 0;

        public CountHandler()
        { }

        protected override void StartRdfInternal()
        {
            this._counter = 0;
        }

        protected override bool HandleTripleInternal(Triple t)
        {
            this._counter++;
            return true;
        }

        /// <summary>
        /// Gets the Count of Triples handled in the most recent parsing operation
        /// </summary>
        /// <remarks>
        /// Note that each time you reuse the handler the count is reset to 0
        /// </remarks>
        public int Count
        {
            get
            {
                return this._counter;
            }
        }

        /// <summary>
        /// Gets that the Handler accepts all Triples
        /// </summary>
        public override bool AcceptsAll
        {
            get 
            {
                return true; 
            }
        }
    }
}
