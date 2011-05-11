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
using VDS.RDF.Parsing.Contexts;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;

namespace VDS.RDF.Parsing
{
    /// <summary>
    /// Parser for SPARQL Boolean results as Plain Text
    /// </summary>
    public class SparqlBooleanParser : ISparqlResultsReader
    {
        /// <summary>
        /// Loads a Result Set from an Input Stream
        /// </summary>
        /// <param name="results">Result Set to load into</param>
        /// <param name="input">Input Stream to read from</param>
        public void Load(SparqlResultSet results, StreamReader input)
        {
            if (results == null) throw new RdfParseException("Cannot read SPARQL Results into a null Result Set");
            this.Load(new ResultSetHandler(results), input);
        }

        /// <summary>
        /// Loads a Result Set from an Input Stream
        /// </summary>
        /// <param name="results">Result Set to load into</param>
        /// <param name="filename">File to read from</param>
        public void Load(SparqlResultSet results, string filename)
        {
            if (results == null) throw new RdfParseException("Cannot read SPARQL Results into a null Result Set");
            if (filename == null) throw new RdfParseException("Cannot read SPARQL Results from a null File");
            this.Load(results, new StreamReader(filename));
        }

        /// <summary>
        /// Loads a Result Set from an Input 
        /// </summary>
        /// <param name="results">Result Set to load into</param>
        /// <param name="input">Input to read from</param>
        public void Load(SparqlResultSet results, TextReader input)
        {
            if (results == null) throw new RdfParseException("Cannot read SPARQL Results into a null Result Set");
            this.Load(new ResultSetHandler(results), input);
        }

        /// <summary>
        /// Loads a Result Set from an Input using a Results Handler
        /// </summary>
        /// <param name="handler">Results Handler to use</param>
        /// <param name="input">Input to read from</param>
        public void Load(ISparqlResultsHandler handler, TextReader input)
        {
            if (handler == null) throw new RdfParseException("Cannot read SPARQL Results using a null Results Handler");
            if (input == null) throw new RdfParseException("Cannot read SPARQL Results from a null Input");

            try
            {
                this.Parse(new BaseResultsParserContext(handler), input);
            }
            catch
            {
                throw;
            }
            finally
            {
                try
                {
                    input.Close();
                }
                catch
                {
                    //No Catch just cleaning up
                }
            }
        }

        /// <summary>
        /// Loads a Result Set from an Input Stream using a Results Handler
        /// </summary>
        /// <param name="handler">Results Handler to use</param>
        /// <param name="input">Input Stream to read from</param>
        public void Load(ISparqlResultsHandler handler, StreamReader input)
        {
            this.Load(handler, (TextReader)input);
        }

        /// <summary>
        /// Loads a Result Set from a file using a Results Handler
        /// </summary>
        /// <param name="handler">Results Handler to use</param>
        /// <param name="filename">File to read from</param>
        public void Load(ISparqlResultsHandler handler, String filename)
        {
            if (filename == null) throw new RdfParseException("Cannot read SPARQL Results from a null File");
            this.Load(handler, new StreamReader(filename));
        }

        private void Parse(IResultsParserContext context, TextReader input)
        {
            this.Parse(context, input.ReadToEnd());
        }

        private void Parse(IResultsParserContext context, String data)
        {
            try
            {
                context.Handler.StartResults();

                bool result;
                if (Boolean.TryParse(data.Trim(), out result))
                {
                    context.Handler.HandleBooleanResult(result);
                }
                else
                {
                    throw new RdfParseException("The input was not a single boolean value as a String");
                }

                context.Handler.EndResults(true);
            }
            catch (RdfParsingTerminatedException)
            {
                context.Handler.EndResults(true);
            }
            catch
            {
                context.Handler.EndResults(false);
                throw;
            }
        }

        /// <summary>
        /// Helper Method which raises the Warning event when a non-fatal issue with the SPARQL Results being parsed is detected
        /// </summary>
        /// <param name="message">Warning Message</param>
        private void RaiseWarning(String message)
        {
            SparqlWarning d = this.Warning;
            if (d != null)
            {
                d(message);
            }
        }

        /// <summary>
        /// Event raised when a non-fatal issue with the SPARQL Results being parsed is detected
        /// </summary>
        public event SparqlWarning Warning;

        /// <summary>
        /// Gets the String representation of the Parser which is a description of the syntax it parses
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "SPARQL Boolean Result";
        }
    }
}
