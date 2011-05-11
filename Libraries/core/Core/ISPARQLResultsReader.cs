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
