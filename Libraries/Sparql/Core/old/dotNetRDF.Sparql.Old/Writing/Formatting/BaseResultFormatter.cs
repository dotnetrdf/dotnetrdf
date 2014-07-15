using System;
using VDS.RDF.Query;

namespace VDS.RDF.Writing.Formatting
{
    /// <summary>
    /// Abstract base class for result formatters
    /// </summary>
    public abstract class BaseResultFormatter
        : BaseFormatter, IResultFormatter
    {
        /// <summary>
        /// Creates a new formatter
        /// </summary>
        /// <param name="formatName">Format Name</param>
        protected BaseResultFormatter(string formatName)
            : base(formatName)
        {
        }

        /// <summary>
        /// Formats a SPARQL Result for the given format
        /// </summary>
        /// <param name="result">SPARQL Result</param>
        /// <returns></returns>
        public virtual String Format(SparqlResult result)
        {
            return result.ToString(this);
        }

        /// <summary>
        /// Formats a SPARQL Boolean Result for the given format
        /// </summary>
        /// <param name="result">Boolean Result</param>
        /// <returns></returns>
        public virtual String FormatBooleanResult(bool result)
        {
            return result.ToString().ToLower();
        }
    }
}
