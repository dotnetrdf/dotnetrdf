using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Writing.Formatting
{
    /// <summary>
    /// Interface for formatters designed to format entire RDF Graphs
    /// </summary>
    public interface IGraphFormatter : ITripleFormatter
    {
        /// <summary>
        /// Generates the header section for the Graph
        /// </summary>
        /// <param name="g">Graph</param>
        /// <returns></returns>
        String FormatGraphHeader(IGraph g);

        /// <summary>
        /// Generates the header section for the Graph
        /// </summary>
        /// <param name="namespaces">Namespaces</param>
        /// <returns></returns>
        String FormatGraphHeader(INamespaceMapper namespaces);

        /// <summary>
        /// Generates a generic header section
        /// </summary>
        /// <returns></returns>
        String FormatGraphHeader();

        /// <summary>
        /// Generates the footer section
        /// </summary>
        /// <returns></returns>
        String FormatGraphFooter();
    }
}
