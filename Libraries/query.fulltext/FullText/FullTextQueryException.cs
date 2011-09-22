using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Query.FullText
{
    /// <summary>
    /// Exception Type for exceptions that may occur during Full Text Query
    /// </summary>
    public class FullTextQueryException
        : RdfQueryException
    {
        /// <summary>
        /// Creates a new Full Text Query Exception
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="cause">Inner Exception</param>
        public FullTextQueryException(String message, Exception cause)
            : base(message, cause) { }

        /// <summary>
        /// Creates a new Full Text Query Exception
        /// </summary>
        /// <param name="message">Message</param>
        public FullTextQueryException(String message)
            : base(message) { }
    }

    /// <summary>
    /// Exception Type for exceptions that may occur during Full Text Indexing
    /// </summary>
    public class FullTextIndexException
        : RdfQueryException
    {
        /// <summary>
        /// Creates a new Full Text Index Exception
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="cause">Inner Exception</param>
        public FullTextIndexException(String message, Exception cause)
            : base(message, cause) { }

        /// <summary>
        /// Creates a new Full Text Index Exception
        /// </summary>
        /// <param name="message">Message</param>
        public FullTextIndexException(String message)
            : base(message) { }
    }
}
