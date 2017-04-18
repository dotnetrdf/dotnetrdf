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
using System.IO;
using VDS.RDF.Parsing.Tokens;

namespace VDS.RDF
{
    /// <summary>
    /// Interface to be implemented by RDF Readers which parse Concrete RDF Syntax
    /// </summary>
    public interface IRdfReader
    {
        /// <summary>
        /// Method for Loading a Graph from some Concrete RDF Syntax via some arbitrary Stream
        /// </summary>
        /// <param name="g">Graph to load RDF into</param>
        /// <param name="input">The reader to read input from</param>
        /// <exception cref="RdfException">Thrown if the Parser tries to output something that is invalid RDF</exception>
        /// <exception cref="Parsing.RdfParseException">Thrown if the Parser cannot Parse the Input</exception>
        /// <exception cref="System.IO.IOException">Thrown if the Parser encounters an IO Error while trying to access/parse the Stream</exception>
        void Load(IGraph g, StreamReader input);

        /// <summary>
        /// Method for Loading a Graph from some Concrete RDF Syntax via some arbitrary Input
        /// </summary>
        /// <param name="g">Graph to load RDF into</param>
        /// <param name="input">The reader to read input from</param>
        /// <exception cref="RdfException">Thrown if the Parser tries to output something that is invalid RDF</exception>
        /// <exception cref="Parsing.RdfParseException">Thrown if the Parser cannot Parse the Input</exception>
        /// <exception cref="System.IO.IOException">Thrown if the Parser encounters an IO Error while trying to access/parse the Stream</exception>
        void Load(IGraph g, TextReader input);

        /// <summary>
        /// Method for Loading a Graph from some Concrete RDF Syntax from a given File
        /// </summary>
        /// <param name="g">Graph to load RDF into</param>
        /// <param name="filename">The Filename of the File to read from</param>
        /// <exception cref="RdfException">Thrown if the Parser tries to output something that is invalid RDF</exception>
        /// <exception cref="Parsing.RdfParseException">Thrown if the Parser cannot Parse the Input</exception>
        /// <exception cref="System.IO.IOException">Thrown if the Parser encounters an IO Error while trying to access/parse the File</exception>
        void Load(IGraph g, String filename);

        /// <summary>
        /// Method for Loading RDF using a RDF Handler from some Concrete RDF Syntax via some arbitrary Stream
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="input">The reader to read input from</param>
        /// <exception cref="RdfException">Thrown if the Parser tries to output something that is invalid RDF</exception>
        /// <exception cref="Parsing.RdfParseException">Thrown if the Parser cannot Parse the Input</exception>
        /// <exception cref="System.IO.IOException">Thrown if the Parser encounters an IO Error while trying to access/parse the Stream</exception>
        void Load(IRdfHandler handler, StreamReader input);

        /// <summary>
        /// Method for Loading RDF using a RDF Handler from some Concrete RDF Syntax via some arbitrary Stream
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="input">The reader to read input from</param>
        /// <exception cref="RdfException">Thrown if the Parser tries to output something that is invalid RDF</exception>
        /// <exception cref="Parsing.RdfParseException">Thrown if the Parser cannot Parse the Input</exception>
        /// <exception cref="System.IO.IOException">Thrown if the Parser encounters an IO Error while trying to access/parse the Stream</exception>
        void Load(IRdfHandler handler, TextReader input);

        /// <summary>
        /// Method for Loading RDF using a RDF Handler from some Concrete RDF Syntax from a given File
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="filename">The Filename of the File to read from</param>
        /// <exception cref="RdfException">Thrown if the Parser tries to output something that is invalid RDF</exception>
        /// <exception cref="Parsing.RdfParseException">Thrown if the Parser cannot Parse the Input</exception>
        /// <exception cref="System.IO.IOException">Thrown if the Parser encounters an IO Error while trying to access/parse the Stream</exception>
        void Load(IRdfHandler handler, String filename);

        /// <summary>
        /// Event which Readers can raise when they notice syntax that is ambigious/deprecated etc which can still be parsed
        /// </summary>
        event RdfReaderWarning Warning;
    }
}

namespace VDS.RDF.Parsing
{
    /// <summary>
    /// Interface for Parsers that support Tokeniser Tracing
    /// </summary>
    public interface ITraceableTokeniser
    {
        /// <summary>
        /// Gets/Sets whether Tokeniser Tracing is used
        /// </summary>
        bool TraceTokeniser
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Interface for Parsers that support Parser Tracing
    /// </summary>
    public interface ITraceableParser 
    {
        /// <summary>
        /// Gets/Sets whether Parser Tracing is used
        /// </summary>
        bool TraceParsing
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Interface for parsers that use token based parsing
    /// </summary>
    public interface ITokenisingParser
    {
        /// <summary>
        /// Gets/Sets the token queue mode used
        /// </summary>
        TokenQueueMode TokenQueueMode
        {
            get;
            set;
        }
    }

}
