/*

Copyright Robert Vesse 2009-10
rvesse@vdesign-studios.com

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

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

}
