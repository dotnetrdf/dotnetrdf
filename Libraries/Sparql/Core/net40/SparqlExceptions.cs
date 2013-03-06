using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace VDS.RDF.Query
{
    /// <summary>
    /// Class for representing errors that occur while querying RDF
    /// </summary>
    public class RdfQueryException : RdfException
    {
        /// <summary>
        /// Creates a new RDF Query Exception
        /// </summary>
        /// <param name="errorMsg">Error Message</param>
        public RdfQueryException(String errorMsg)
            : base(errorMsg) { }

        /// <summary>
        /// Creates a new RDF Query Exception
        /// </summary>
        /// <param name="errorMsg">Error Message</param>
        /// <param name="cause">Exception that caused this Exception</param>
        public RdfQueryException(String errorMsg, Exception cause)
            : base(errorMsg, cause) { }
    }

    /// <summary>
    /// Class for representing Timeout errors that occur while querying RDF
    /// </summary>
    public class RdfQueryTimeoutException : RdfQueryException
    {
        /// <summary>
        /// Creates a new RDF Query Timeout Exception
        /// </summary>
        /// <param name="errorMsg">Error Message</param>
        public RdfQueryTimeoutException(String errorMsg)
            : base(errorMsg) { }
    }

    /// <summary>
    /// Class for representing Exceptions occurring in RDF reasoners
    /// </summary>
    public class RdfReasoningException : RdfQueryException
    {
        /// <summary>
        /// Creates a new RDF Reasoning Exception
        /// </summary>
        /// <param name="errorMsg">Error Message</param>
        public RdfReasoningException(String errorMsg)
            : base(errorMsg) { }

        /// <summary>
        /// Creates a new RDF Reasoning Exception
        /// </summary>
        /// <param name="errorMsg">Error Message</param>
        /// <param name="cause">Exception that caused this exception</param>
        public RdfReasoningException(String errorMsg, Exception cause)
            : base(errorMsg, cause) { }
    }

    /// <summary>
    /// Class for representing Termination errors
    /// </summary>
    class RdfQueryTerminatedException : RdfQueryException
    {
        /// <summary>
        /// Creates a new RDF Query Termination Exception
        /// </summary>
        public RdfQueryTerminatedException()
            : base("Terminated Query since there are no results at the point reached so further execution is unnecessary") { }
    }

    /// <summary>
    /// Class for representing Path Found terminations
    /// </summary>
    class RdfQueryPathFoundException : RdfQueryException
    {
        /// <summary>
        /// Creates a new Path Found exception
        /// </summary>
        public RdfQueryPathFoundException()
            : base("Terminated Path Evaluation since the required path has been found") { }
    }
}


namespace VDS.RDF.Update
{
    /// <summary>
    /// Class of exceptions that may occur when performing SPARQL Updates
    /// </summary>
    public class SparqlUpdateException : RdfException
    {
        /// <summary>
        /// Creates a new RDF Update Exception
        /// </summary>
        /// <param name="message">Error Message</param>
        public SparqlUpdateException(String message)
            : base(message) { }

        /// <summary>
        /// Createa a new RDF Update Exception
        /// </summary>
        /// <param name="message">Error Message</param>
        /// <param name="cause">Exception that caused this exception to be thrown</param>
        public SparqlUpdateException(String message, Exception cause)
            : base(message, cause) { }
    }

    /// <summary>
    /// Class for representing Timeout errors that occur while updating RDF using SPARQL
    /// </summary>
    public class SparqlUpdateTimeoutException
        : SparqlUpdateException
    {
        /// <summary>
        /// Creates a new SPARQL Update Timeout Exception
        /// </summary>
        /// <param name="errorMsg">Error Message</param>
        public SparqlUpdateTimeoutException(String errorMsg)
            : base(errorMsg) { }
    }

    /// <summary>
    /// Class for representing Permissions errors with SPARQL Updates
    /// </summary>
    public class SparqlUpdatePermissionException
        : SparqlUpdateException
    {
        /// <summary>
        /// Creates a new Permission Exception
        /// </summary>
        /// <param name="message">Error Message</param>
        public SparqlUpdatePermissionException(String message)
            : base(message) { }

        /// <summary>
        /// Creates a new Permission Exception
        /// </summary>
        /// <param name="message">Error Message</param>
        /// <param name="cause">Exception that caused this exception to be thrown</param>
        public SparqlUpdatePermissionException(String message, Exception cause)
            : base(message, cause) { }
    }

    /// <summary>
    /// Class for representing malformed SPARQL Updates
    /// </summary>
    /// <remarks>
    /// This is distinct from a <see cref="VDS.RDF.Parsing.RdfParseException">RdfParseException</see> as it is possible for an update to be syntactically valid but semantically malformed
    /// </remarks>
    public class SparqlUpdateMalformedException
        : SparqlUpdateException
    {
        /// <summary>
        /// Creates a new Malformed Update Exception
        /// </summary>
        /// <param name="message">Error Message</param>
        public SparqlUpdateMalformedException(String message)
            : base(message) { }

        /// <summary>
        /// Creates a new Malformed Update Exception
        /// </summary>
        /// <param name="message">Error Message</param>
        /// <param name="cause">Exception that caused this exception to be thrown</param>
        public SparqlUpdateMalformedException(String message, Exception cause)
            : base(message, cause) { }
    }
}

namespace VDS.RDF.Update.Protocol
{
    /// <summary>
    /// Class of exceptions that may occur when using the SPARQL Graph Store HTTP Protocol for Graph Management
    /// </summary>
    public class SparqlHttpProtocolException : RdfException
    {
        /// <summary>
        /// Creates a new SPARQL Graph Store HTTP Protocol Exception
        /// </summary>
        /// <param name="message">Error Message</param>
        public SparqlHttpProtocolException(String message)
            : base(message) { }

        /// <summary>
        /// Creates a new SPARQL Graph Store HTTP Protocol Exception
        /// </summary>
        /// <param name="message">Error Message</param>
        /// <param name="cause">Exception that caused this Exception</param>
        public SparqlHttpProtocolException(String message, Exception cause)
            : base(message, cause) { }
    }

    /// <summary>
    /// Exception that occurs when a Protocol Processor cannot resolve the URI for the Graph to be acted upon
    /// </summary>
    public class SparqlHttpProtocolUriResolutionException : SparqlHttpProtocolException
    {
        /// <summary>
        /// Creates a new Protocol URI Resolution Exception
        /// </summary>
        public SparqlHttpProtocolUriResolutionException()
            : base("Unable to perform a HTTP Protocol operation as the protocol processor was unable to successfully resolve the URI of the Graph to be acted upon") { }

        /// <summary>
        /// Creates a new Protocol URI Resolution Exception
        /// </summary>
        /// <param name="message">Error Message</param>
        public SparqlHttpProtocolUriResolutionException(String message)
            : base(message) { }
    }

    /// <summary>
    /// Exception that occurs when a Protocol Processor is provided with a invalid URI for the Graph to be acted upon
    /// </summary>
    public class SparqlHttpProtocolUriInvalidException : SparqlHttpProtocolException
    {
        /// <summary>
        /// Creates a new Protocol Invalid URI Exception
        /// </summary>
        public SparqlHttpProtocolUriInvalidException()
            : base("Unable to perform a HTTP Protocol operation as the request specified a URI which was invalid") { }
    }
}