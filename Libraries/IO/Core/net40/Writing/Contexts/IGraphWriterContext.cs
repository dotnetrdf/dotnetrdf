using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Graphs;

namespace VDS.RDF.Writing.Contexts
{
    public interface IGraphWriterContext
        : IWriterContext
    {
        /// <summary>
        /// Gets the Graph being written
        /// </summary>
        IGraph Graph
        {
            get;
        }
    }
}
