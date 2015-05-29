/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-develop@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
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

namespace VDS.RDF.Nodes
{
    /// <summary>
    /// Exception that may occur when working with <see cref="IValuedNode"/> instances
    /// </summary>
    public class NodeValueException
        : RdfException
    {
        /// <summary>
        /// Creates a new Node Value Exception with the given Message
        /// </summary>
        /// <param name="errorMsg">Error Message</param>
        public NodeValueException(String errorMsg)
            : base(errorMsg) { }

        /// <summary>
        /// Creates a new Node Value Exception with the given Message and Inner Exception
        /// </summary>
        /// <param name="errorMsg">Error Message</param>
        /// <param name="cause">Inner Exception</param>
        public NodeValueException(String errorMsg, Exception cause)
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