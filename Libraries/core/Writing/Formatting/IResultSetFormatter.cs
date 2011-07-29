using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query;

namespace VDS.RDF.Writing.Formatting
{
    /// <summary>
    /// Interface for formatters designed to format entire SPARQL Result Sets
    /// </summary>
    public interface IResultSetFormatter : IResultFormatter
    {
        /// <summary>
        /// Generates a header section using the given variables
        /// </summary>
        /// <param name="variables">Variables</param>
        /// <returns></returns>
        String FormatResultSetHeader(IEnumerable<String> variables);

        /// <summary>
        /// Generates a header section assuming no variables
        /// </summary>
        /// <returns></returns>
        String FormatResultSetHeader();

        /// <summary>
        /// Generates a footer section
        /// </summary>
        /// <returns></returns>
        String FormatResultSetFooter();
    }
}
