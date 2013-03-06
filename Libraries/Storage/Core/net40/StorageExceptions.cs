using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Storage
{
    /// <summary>
    /// Class for representing errors that occur in RDF Storage
    /// </summary>
    public class RdfStorageException : RdfException
    {
        /// <summary>
        /// Creates a new RDF Storage Exception
        /// </summary>
        /// <param name="errorMsg">Error Message</param>
        public RdfStorageException(String errorMsg)
            : base(errorMsg) { }

        /// <summary>
        /// Creates a new RDF Storage Exception
        /// </summary>
        /// <param name="errorMsg">Error Message</param>
        /// <param name="cause">Exception which caused this Exception</param>
        public RdfStorageException(String errorMsg, Exception cause)
            : base(errorMsg, cause) { }
    }
}
