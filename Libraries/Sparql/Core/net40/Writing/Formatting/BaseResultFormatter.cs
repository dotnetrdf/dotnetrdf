using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query;

namespace VDS.RDF.Writing.Formatting
{
    public class BaseResultFormatter
        : BaseFormatter, IResultFormatter
    {
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
