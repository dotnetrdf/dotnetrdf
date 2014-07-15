using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Graphs;
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Templates
{
    /// <summary>
    /// Interface for templates
    /// </summary>
    public interface ITemplate
    {
        /// <summary>
        /// Gets the quads that this template represents
        /// </summary>
        IEnumerable<Quad> TemplateQuads { get; }

        /// <summary>
        /// Gets the names of graphs affected by this template
        /// </summary>
        IEnumerable<INode> TemplateGraphs { get; } 
    }
}
