using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Graphs;
using VDS.RDF.Nodes;

namespace VDS.RDF.Writing.Contexts
{
    public interface IGraphStoreWriterContext
        : IWriterContext
    {
        /// <summary>
        /// Gets the graph store being written
        /// </summary>
        IGraphStore GraphStore { get; }

        /// <summary>
        /// Gets the current graph
        /// </summary>
        IGraph CurrentGraph { get; set; }

        /// <summary>
        /// Gets the current graph name
        /// </summary>
        INode CurrentGraphName { get; set; }
    }
}
