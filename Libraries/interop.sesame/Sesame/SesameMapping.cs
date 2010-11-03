using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dotSesame = org.openrdf.model;

namespace VDS.RDF.Interop.Sesame
{
    public class SesameMapping
    {
        private IGraph _g;
        private dotSesame.Graph _target;
        private dotSesame.ValueFactory _factory;
        private Dictionary<dotSesame.BNode, INode> _inputMapping = new Dictionary<dotSesame.BNode, INode>();
        private Dictionary<INode, dotSesame.BNode> _outputMapping = new Dictionary<INode, dotSesame.BNode>();

        /// <summary>
        /// Creates a new Sesame mapping
        /// </summary>
        /// <param name="g">Graph</param>
        public SesameMapping(IGraph g, dotSesame.Graph target)
        {
            this._g = g;
            this._target = target;
        }

        public SesameMapping(DotNetRdfValueFactory factory, dotSesame.Graph target)
        {
            this._g = factory.Graph;
            this._target = target;
            this._factory = factory;
        }

        /// <summary>
        /// Gets the Source Graph to which this mapping applies
        /// </summary>
        public IGraph Graph
        {
            get
            {
                return this._g;
            }
        }

        /// <summary>
        /// Gets the Target Graph to which this mapping applies
        /// </summary>
        public dotSesame.Graph Target
        {
            get
            {
                return this._target;
            }
        }

        /// <summary>
        /// Gets the Value Factory used to mint Values for a Sesame model
        /// </summary>
        public dotSesame.ValueFactory ValueFactory
        {
            get
            {
                if (this._factory == null) this._factory = this._target.getValueFactory();
                return this._factory;
            }
        }

        /// <summary>
        /// Gets the mapping from Sesame Blank Nodes to dotNetRDF Blank Nodes
        /// </summary>
        public Dictionary<dotSesame.BNode, INode> InputMapping
        {
            get
            {
                return this._inputMapping;
            }
        }

        /// <summary>
        /// Gets the mapping from dotNetRDF Blank Nodes to Semsame Blank Node
        /// </summary>
        public Dictionary<INode, dotSesame.BNode> OutputMapping
        {
            get
            {
                return this._outputMapping;
            }
        }
    }
}
