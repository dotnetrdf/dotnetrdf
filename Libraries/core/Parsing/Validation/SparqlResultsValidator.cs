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
using VDS.RDF.Query;

namespace VDS.RDF.Parsing.Validation
{
    /// <summary>
    /// Syntax Validator which validates SPARQL Results formats
    /// </summary>
    public class SparqlResultsValidator : ISyntaxValidator
    {
        private ISparqlResultsReader _parser;

        /// <summary>
        /// Creates a new SPARQL Results Format validator that uses the given parser
        /// </summary>
        /// <param name="parser">SPARQL Results Parser</param>
        public SparqlResultsValidator(ISparqlResultsReader parser)
        {
            this._parser = parser;
        }

        /// <summary>
        /// Validates the syntax to see if it is valid SPARQL Results
        /// </summary>
        /// <param name="data">Data to validate</param>
        /// <returns></returns>
        public ISyntaxValidationResults Validate(string data)
        {
            String message;
            try
            {
                SparqlResultSet results = new SparqlResultSet();
                StringParser.ParseResultSet(results, data);
                message = "Valid SPARQL Results - " + results.Count + " Results - Parser: " + this._parser.GetType().Name;

                return new SyntaxValidationResults(true, message, results);
            }
            catch (RdfParseException parseEx)
            {
                message = "Invalid SPARQL Results - Parsing Error from Parser: " + this._parser.GetType().Name + " - " + parseEx.Message;
                return new SyntaxValidationResults(message, parseEx);
            }
            catch (RdfException rdfEx)
            {
                message = "Invalid SPARQL Results - RDF Error from Parser: " + this._parser.GetType().Name + " - " + rdfEx.Message;
                return new SyntaxValidationResults(message, rdfEx);
            }
            catch (Exception ex)
            {
                message = "Invalid SPARQL Results - Error from Parser: " + this._parser.GetType().Name + " - " + ex.Message;
                return new SyntaxValidationResults(message, ex);
            }
        }
    }
}
