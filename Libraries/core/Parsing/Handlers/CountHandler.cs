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

        public int Count
        {
            get
            {
                return this._counter;
            }
        }

        public override bool AcceptsAll
        {
            get 
            {
                return true; 
            }
        }
    }
}
