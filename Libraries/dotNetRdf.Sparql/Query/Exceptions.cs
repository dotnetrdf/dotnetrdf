using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDS.RDF.Query
{
    /// <summary>
    /// Class for representing Termination errors.
    /// </summary>
    class RdfQueryTerminatedException : RdfQueryException
    {
        /// <summary>
        /// Creates a new RDF Query Termination Exception.
        /// </summary>
        public RdfQueryTerminatedException()
            : base("Terminated Query since there are no results at the point reached so further execution is unnecessary") { }
    }

    /// <summary>
    /// Class for representing Path Found terminations.
    /// </summary>
    class RdfQueryPathFoundException : RdfQueryException
    {
        /// <summary>
        /// Creates a new Path Found exception.
        /// </summary>
        public RdfQueryPathFoundException()
            : base("Terminated Path Evaluation since the required path has been found") { }
    }
}
