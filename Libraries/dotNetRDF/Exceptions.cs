/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

using System;
using System.Collections.Generic;
using VDS.RDF.Parsing.Tokens;

namespace VDS.RDF
{
    /// <summary>
    /// Class for representing errors with RDF
    /// </summary>
    public class RdfException : Exception
    {
        /// <summary>
        /// Creates a new RDF Exception with the given Message
        /// </summary>
        /// <param name="errorMsg">Error Message</param>
        public RdfException(String errorMsg)
            : base(errorMsg) { }

        /// <summary>
        /// Creates a new RDF Exception with the given Message and Inner Exception
        /// </summary>
        /// <param name="errorMsg">Error Message</param>
        /// <param name="cause">Inner Exception</param>
        public RdfException(String errorMsg, Exception cause)
            : base(errorMsg, cause) { }
    }

}

namespace VDS.RDF.Configuration
{

    /// <summary>
    /// Class for representing errors with dotNetRDF Configuration
    /// </summary>
    /// <remarks>
    /// <para>
    /// Configuration exceptions are thrown when the user tries to load objects using the <see cref="ConfigurationLoader">ConfigurationLoader</see> and their is insufficient/invalid information to load the desired object
    /// </para>
    /// </remarks>
    public class DotNetRdfConfigurationException : RdfException
    {
        /// <summary>
        /// Creates a new dotNetRDF Configuration Exception
        /// </summary>
        /// <param name="errorMsg">Error Message</param>
        public DotNetRdfConfigurationException(String errorMsg)
            : base(errorMsg) { }

        /// <summary>
        /// Creates a new dotNetRDF Configuration Exception
        /// </summary>
        /// <param name="errorMsg">Error Message</param>
        /// <param name="cause">Exception that caused this Exception</param>
        public DotNetRdfConfigurationException(String errorMsg, Exception cause)
            : base(errorMsg, cause) { }
    }
}

namespace VDS.RDF.Ontology
{
    /// <summary>
    /// Class for representing errors with Ontologies
    /// </summary>
    public class RdfOntologyException : RdfException
    {
        /// <summary>
        /// Creates a new RDF Ontology Exception with the given message
        /// </summary>
        /// <param name="errorMsg">Error message</param>
        public RdfOntologyException(String errorMsg)
            : base(errorMsg) { }

        /// <summary>
        /// Creates a new RDF Ontology Exception with the given message and inner exception
        /// </summary>
        /// <param name="errorMsg">Error message</param>
        /// <param name="cause">Inner Exception</param>
        public RdfOntologyException(String errorMsg, Exception cause)
            : base(errorMsg, cause) { }
    }
}

namespace VDS.RDF.Parsing
{
    /// <summary>
    /// Class for representing errors in parsing RDF
    /// </summary>
    public class RdfParseException : RdfException
    {
        private bool _hasPositionInfo = false;
        private int _startLine, _endLine, _startPos, _endPos;

        /// <summary>
        /// Creates a new RDF Parse Exception with the given Message
        /// </summary>
        /// <param name="errorMsg">Error Message</param>
        public RdfParseException(String errorMsg)
            : base(errorMsg) { }

        /// <summary>
        /// Creates a new RDF Parse Exception with the given Message and Inner Exception
        /// </summary>
        /// <param name="errorMsg">Error Message</param>
        /// <param name="cause">Inner Exception</param>
        public RdfParseException(String errorMsg, Exception cause)
            : base(errorMsg, cause) { }

        /// <summary>
        /// Creates a new RDF Parse Exception which contains Position Information taken from the given Token
        /// </summary>
        /// <param name="errorMsg">Error Message</param>
        /// <param name="t">Token</param>
        public RdfParseException(String errorMsg, IToken t)
            : this(errorMsg, t, null) { }

        /// <summary>
        /// Creates a new RDF Parse Exception which contains Position Information taken from the given Token
        /// </summary>
        /// <param name="errorMsg">Error Message</param>
        /// <param name="t">Token</param>
        /// <param name="cause">Inner Exception</param>
        public RdfParseException(String errorMsg, IToken t, Exception cause)
            : base(errorMsg, cause)
        {
            if (t != null)
            {
                _hasPositionInfo = true;
                _startLine = t.StartLine;
                _endLine = t.EndLine;
                _startPos = t.StartPosition;
                _endPos = t.EndPosition;
            }
        }

        /// <summary>
        /// Creates a new RDF Parse Exception which contains Position Information
        /// </summary>
        /// <param name="errorMsg">Error Message</param>
        /// <param name="line">Line the error occurred on</param>
        /// <param name="pos">Column Position the error occurred at</param>
        /// <param name="cause">Exeception that caused this exception</param>
        public RdfParseException(String errorMsg, int line, int pos, Exception cause)
            : base(errorMsg, cause)
        {
            _hasPositionInfo = true;
            _startLine = _endLine = line;
            _startPos = _endPos = pos;
        }

        /// <summary>
        /// Creates a new RDF Parse Exception which contains Position Information
        /// </summary>
        /// <param name="errorMsg">Error Message</param>
        /// <param name="line">Line the error occurred on</param>
        /// <param name="pos">Column Position the error occurred at</param>
        public RdfParseException(String errorMsg, int line, int pos)
            : this(errorMsg, line, pos, null) { }

        /// <summary>
        /// Creates a new RDF Parse Exception which contains Position Information
        /// </summary>
        /// <param name="errorMsg">Error Message</param>
        /// <param name="line">Line the error occurred on</param>
        /// <param name="startPos">Column Position the error starts at</param>
        /// <param name="endPos">Column Position the error ends at</param>
        /// <param name="cause">Error that caused this exception</param>
        public RdfParseException(String errorMsg, int line, int startPos, int endPos, Exception cause)
            : this(errorMsg, line, startPos, cause)
        {
            _endPos = endPos;
        }

        /// <summary>
        /// Creates a new RDF Parse Exception which contains Position Information
        /// </summary>
        /// <param name="errorMsg">Error Message</param>
        /// <param name="line">Line the error occurred on</param>
        /// <param name="startPos">Column Position the error starts at</param>
        /// <param name="endPos">Column Position the error ends at</param>
        public RdfParseException(String errorMsg, int line, int startPos, int endPos)
            : this(errorMsg, line, startPos, endPos, null) { }

        /// <summary>
        /// Creates a new RDF Parse Exception which contains Position Information
        /// </summary>
        /// <param name="errorMsg">Error Message</param>
        /// <param name="startLine">Line the error starts on</param>
        /// <param name="endLine">Line the error ends on</param>
        /// <param name="startPos">Column Position the error starts at</param>
        /// <param name="endPos">Column Position the error ends at</param>
        /// <param name="cause">Error that caused this exception</param>
        public RdfParseException(String errorMsg, int startLine, int endLine, int startPos, int endPos, Exception cause)
            : this(errorMsg, startLine, startPos, cause)
        {
            _endLine = endLine;
            _endPos = endPos;
        }

        /// <summary>
        /// Creates a new RDF Parse Exception which contains Position Information
        /// </summary>
        /// <param name="errorMsg">Error Message</param>
        /// <param name="startLine">Line the error starts on</param>
        /// <param name="endLine">Line the error ends on</param>
        /// <param name="startPos">Column Position the error starts at</param>
        /// <param name="endPos">Column Position the error ends at</param>
        public RdfParseException(String errorMsg, int startLine, int endLine, int startPos, int endPos)
            : this(errorMsg, startLine, endLine, startPos, endPos, null) { }

        /// <summary>
        /// Creates a new RDF Parse Exception which contains Position Information
        /// </summary>
        /// <param name="errorMsg">Error Message</param>
        /// <param name="position">Position Information</param>
        /// <param name="cause">Error that caused this exception</param>
        public RdfParseException(String errorMsg, PositionInfo position, Exception cause)
            : this(errorMsg, position.StartLine, position.EndLine, position.StartPosition, position.EndPosition, cause) { }

        /// <summary>
        /// Creates a new RDF Parse Exception which contains Position Information
        /// </summary>
        /// <param name="errorMsg">Error Message</param>
        /// <param name="position">Position Information</param>
        public RdfParseException(String errorMsg, PositionInfo position)
            : this(errorMsg, position, null) { }

        /// <summary>
        /// Gets whether the Exception has any position information
        /// </summary>
        public bool HasPositionInformation
        {
            get
            {
                return _hasPositionInfo;
            }
        }

        /// <summary>
        /// Gets the Start Line of the Error or -1 if no position information
        /// </summary>
        public int StartLine
        {
            get
            {
                return (_hasPositionInfo) ? _startLine : -1;
            }
        }

        /// <summary>
        /// Gets the End Line of the Error or -1 if no position information
        /// </summary>
        public int EndLine
        {
            get
            {
                return (_hasPositionInfo) ? _endLine : -1;
            }
        }

        /// <summary>
        /// Gets the Start Column of the Error or -1 if no position information
        /// </summary>
        public int StartPosition
        {
            get
            {
                return (_hasPositionInfo) ? _startPos : -1;
            }
        }

        /// <summary>
        /// Gets the End Column of the Error or -1 if no position information
        /// </summary>
        public int EndPosition
        {
            get
            {
                return (_hasPositionInfo) ? _endPos : -1;
            }
        }
    }

    /// <summary>
    /// Class of exceptions that may occur when doing multi-threaded parsing of RDF
    /// </summary>
    /// <remarks>
    /// <para>
    /// Used when a process may result in multiple errors from different threads
    /// </para>
    /// </remarks>
    public class RdfThreadedParsingException : RdfParseException
    {
        private List<Exception> _exceptions = new List<Exception>();

        /// <summary>
        /// Creates a new Threaded RDF Parsing Exception
        /// </summary>
        /// <param name="message">Error Message</param>
        public RdfThreadedParsingException(String message)
            : base(message) { }

        /// <summary>
        /// Adds an Exception to the list of Inner Exceptions
        /// </summary>
        /// <param name="ex">Exception</param>
        public void AddException(Exception ex)
        {
            _exceptions.Add(ex);
        }

        /// <summary>
        /// Gets the enumeration of Exceptions
        /// </summary>
        public IEnumerable<Exception> InnerExceptions
        {
            get
            {
                return _exceptions;
            }
        }
    }

    /// <summary>
    /// Class for representing errors in selecting an appropriate parser to parse RDF with
    /// </summary>
    public class RdfParserSelectionException : RdfParseException
    {
        /// <summary>
        /// Creates a new RDF Parser Selection Exception with the given Message
        /// </summary>
        /// <param name="errorMsg">Error Message</param>
        public RdfParserSelectionException(String errorMsg)
            : base(errorMsg) { }

        /// <summary>
        /// Creates a new RDF Parser Selection Exception with the given Message and Inner Exception
        /// </summary>
        /// <param name="errorMsg">Error Message</param>
        /// <param name="cause">Inner Exception</param>
        public RdfParserSelectionException(String errorMsg, Exception cause)
            : base(errorMsg, cause) { }
    }

    /// <summary>
    /// Class for representing that a parser has been terminated by a <see cref="IRdfHandler">IRdfHandler</see>
    /// </summary>
    /// <remarks>
    /// Used internally to help force execution to jump back to the point where we can handle by safely discarding this exception and stop parsing
    /// </remarks>
    public class RdfParsingTerminatedException : RdfParseException
    {
        /// <summary>
        /// Creates a new Parsing Terminated exception
        /// </summary>
        public RdfParsingTerminatedException()
            : base("Parsing was Terminated as the IRdfHandler handling the parsed RDF indicated that it wanted the parser to stop") { }
    }
}

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

namespace VDS.RDF.Skos
{
    public class RdfSkosException : RdfException
    {
        public RdfSkosException(string errorMsg) : base(errorMsg) { }

        public RdfSkosException(string errorMsg, Exception cause) : base(errorMsg, cause) { }
    }
}

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

namespace VDS.RDF.Writing
{
    /// <summary>
    /// Class of exceptions that may occur when outputting RDF
    /// </summary>
    public class RdfOutputException : RdfException
    {
        /// <summary>
        /// Creates a new RDF Output Exception
        /// </summary>
        /// <param name="message">Error Message</param>
        public RdfOutputException(String message)
            : base(message) { }

        /// <summary>
        /// Creates a new RDF Output Exception
        /// </summary>
        /// <param name="message">Error Message</param>
        /// <param name="cause">Exception that caused this Exception</param>
        public RdfOutputException(String message, Exception cause)
            : base(message, cause) { }
    }

    /// <summary>
    /// Class of exceptions that may occur when doing multi-threaded output of RDF
    /// </summary>
    /// <remarks>
    /// <para>
    /// Used when a process may result in multiple errors from different threads
    /// </para>
    /// </remarks>
    public class RdfThreadedOutputException : RdfOutputException
    {
        private List<Exception> _exceptions = new List<Exception>();

        /// <summary>
        /// Creates a new Threaded RDF Output Exception
        /// </summary>
        /// <param name="message">Error Message</param>
        public RdfThreadedOutputException(String message)
            : base(message) { }

        /// <summary>
        /// Adds an Exception to the list of Inner Exceptions
        /// </summary>
        /// <param name="ex">Exception</param>
        public void AddException(Exception ex)
        {
            _exceptions.Add(ex);
        }

        /// <summary>
        /// Gets the enumeration of Exceptions
        /// </summary>
        public IEnumerable<Exception> InnerExceptions
        {
            get
            {
                return _exceptions;
            }
        }
    }

    /// <summary>
    /// Class for errors in selecting an appropriate Writer to output RDF with
    /// </summary>
    public class RdfWriterSelectionException : RdfOutputException
    {
        /// <summary>
        /// Creates a new RDF Writer Selection Exception with the given Message
        /// </summary>
        /// <param name="errorMsg">Error Message</param>
        public RdfWriterSelectionException(String errorMsg) : base(errorMsg) { }

        /// <summary>
        /// Creates a new RDF Writer Selection Exception with the given Message and Inner Exception
        /// </summary>
        /// <param name="errorMsg">Error Message</param>
        /// <param name="cause">Inner Exception</param>
        public RdfWriterSelectionException(String errorMsg, Exception cause) : base(errorMsg, cause) { }
    }
}