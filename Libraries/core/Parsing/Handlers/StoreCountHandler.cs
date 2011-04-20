using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Parsing.Handlers
{
    /// <summary>
    /// A RDF Handler which simply counts the Triples and Graphs
    /// </summary>
    public class StoreCountHandler : BaseRdfHandler
    {
        private int _counter = 0;
        private HashSet<String> _graphs;

        public StoreCountHandler()
        { }

        protected override void StartRdfInternal()
        {
            this._counter = 0;
            this._graphs = new HashSet<string>();
        }

        protected override bool HandleTripleInternal(Triple t)
        {
            this._counter++;
            this._graphs.Add(t.GraphUri.ToSafeString());
            return true;
        }

        public int TripleCount
        {
            get
            {
                return this._counter;
            }
        }

        public int GraphCount
        {
            get 
            {
                return this._graphs.Count;
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
