using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dotSesame = org.openrdf.model;

namespace VDS.RDF.Interop.Sesame
{
    public class SesameGraph : Graph
    {
        private dotSesame.Graph _g;
        private SesameMapping _mapping;

        public SesameGraph(dotSesame.Graph g)
        {
            this._g = g;
            this._mapping = new SesameMapping(this, this._g);
            this._triples = new SesameTripleCollection(this._g, this._mapping);
        }

        public override IBlankNode CreateBlankNode()
        {
            IBlankNode n = base.CreateBlankNode();
            dotSesame.BNode bnode = this._mapping.ValueFactory.createBNode();

            lock (this._mapping)
            {
                if (!this._mapping.OutputMapping.ContainsKey(n)) this._mapping.OutputMapping.Add(n, bnode);
                if (!this._mapping.InputMapping.ContainsKey(bnode)) this._mapping.InputMapping.Add(bnode, n);
            }

            return n;
        }

        public override IBlankNode CreateBlankNode(string nodeId)
        {
            IBlankNode n = this.GetBlankNode(nodeId);
            if (n == null) n = this.CreateBlankNode(nodeId);
            dotSesame.BNode bnode = this._mapping.ValueFactory.createBNode(nodeId);

            lock (this._mapping)
            {
                if (!this._mapping.OutputMapping.ContainsKey(n)) this._mapping.OutputMapping.Add(n, bnode);
                if (!this._mapping.InputMapping.ContainsKey(bnode)) this._mapping.InputMapping.Add(bnode, n);
            }

            return n;
        }
    }
}
