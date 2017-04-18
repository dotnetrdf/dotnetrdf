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
using VDS.RDF.Query;

namespace VDS.RDF
{
    /// <summary>
    /// Interface for Reader Classes which parser Sparql Result Set syntaxes into Result Set objects
    /// </summary>
    public interface ISparqlResultsReader
    {
        /// <summary>
        /// Loads a Result Set from the given Stream
        /// </summary>
        /// <param name="input">Stream to read from</param>
        /// <param name="results">Result Set to load into</param>
        /// <returns></returns>
        /// <remarks>Should throw an error if the Result Set is not empty</remarks>
        void Load(SparqlResultSet results, StreamReader input);

        /// <summary>
        /// Loads a Result Set from the given File
        /// </summary>
        /// <param name="filename">File containing a Result Set</param>
        /// <param name="results">Result Set to load into</param>
        /// <returns></returns>
        /// <remarks>Should throw an error if the Result Set is not empty</remarks>
        void Load(SparqlResultSet results, String filename);

        /// <summary>
        /// Loads a Result Set from the given Input
        /// </summary>
        /// <param name="input">Input to read from</param>
        /// <param name="results">Result Set to load into</param>
        /// <returns></returns>
        /// <remarks>Should throw an error if the Result Set is not empty</remarks>
        void Load(SparqlResultSet results, TextReader input);

        /// <summary>
        /// Loads a Result Set using a Results Handler from the given Stream
        /// </summary>
        /// <param name="handler">Results Handler</param>
        /// <param name="input">Stream to read from</param>
        void Load(ISparqlResultsHandler handler, StreamReader input);

        /// <summary>
        /// Loads a Result Set using a Results Handler from the given file
        /// </summary>
        /// <param name="handler">Results Handler</param>
        /// <param name="filename">File to read results from</param>
        void Load(ISparqlResultsHandler handler, String filename);

        /// <summary>
        /// Loads a Result Set using a Results Handler from the given Input
        /// </summary>
        /// <param name="handler">Results Handler</param>
        /// <param name="input">Input to read from</param>
        void Load(ISparqlResultsHandler handler, TextReader input);

        /// <summary>
        /// Event raised when a non-fatal issue with the SPARQL Results being parsed is detected
        /// </summary>
        event SparqlWarning Warning;
    }
}
