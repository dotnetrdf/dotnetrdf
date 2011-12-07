using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.hp.hpl.jena.rdf.model;

namespace VDS.RDF.Interop.Jena
{
	public class JenaGraph : Graph
	{
        /// <summary>
        /// Creates a new Jena Graph which is a dotNetRDF wrapper around a Jena Model
        /// </summary>
        /// <param name="m">Model</param>
        public JenaGraph(Model m)
        {
            this._triples = new JenaTripleCollection(this, m);
        }
	}
}
