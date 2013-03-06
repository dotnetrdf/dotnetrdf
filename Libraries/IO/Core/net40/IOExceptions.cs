using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
                this._hasPositionInfo = true;
                this._startLine = t.StartLine;
                this._endLine = t.EndLine;
                this._startPos = t.StartPosition;
                this._endPos = t.EndPosition;
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
            this._hasPositionInfo = true;
            this._startLine = this._endLine = line;
            this._startPos = this._endPos = pos;
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
            this._endPos = endPos;
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
            this._endLine = endLine;
            this._endPos = endPos;
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
                return this._hasPositionInfo;
            }
        }

        /// <summary>
        /// Gets the Start Line of the Error or -1 if no position information
        /// </summary>
        public int StartLine
        {
            get
            {
                return (this._hasPositionInfo) ? this._startLine : -1;
            }
        }

        /// <summary>
        /// Gets the End Line of the Error or -1 if no position information
        /// </summary>
        public int EndLine
        {
            get
            {
                return (this._hasPositionInfo) ? this._endLine : -1;
            }
        }

        /// <summary>
        /// Gets the Start Column of the Error or -1 if no position information
        /// </summary>
        public int StartPosition
        {
            get
            {
                return (this._hasPositionInfo) ? this._startPos : -1;
            }
        }

        /// <summary>
        /// Gets the End Column of the Error or -1 if no position information
        /// </summary>
        public int EndPosition
        {
            get
            {
                return (this._hasPositionInfo) ? this._endPos : -1;
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
            this._exceptions.Add(ex);
        }

        /// <summary>
        /// Gets the enumeration of Exceptions
        /// </summary>
        public IEnumerable<Exception> InnerExceptions
        {
            get
            {
                return this._exceptions;
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
