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
    /// Syntax Validator for SPARQL Queries
    /// </summary>
    public class SparqlQueryValidator : ISyntaxValidator
    {
        private SparqlQueryParser _parser;

        /// <summary>
        /// Creates a new SPARQL Query Validator
        /// </summary>
        public SparqlQueryValidator()
        {
            this._parser = new SparqlQueryParser();
        }

        /// <summary>
        /// Creates a new SPARQL Query Validator using the given Syntax
        /// </summary>
        /// <param name="syntax">Query Syntax</param>
        public SparqlQueryValidator(SparqlQuerySyntax syntax)
        {
            this._parser = new SparqlQueryParser(syntax);
        }

        /// <summary>
        /// Creates a new SPARQL Query Validator using the given Query Parser
        /// </summary>
        /// <param name="parser">Query Parser</param>
        public SparqlQueryValidator(SparqlQueryParser parser)
        {
            this._parser = parser;
        }

        /// <summary>
        /// Validates whether the given Data is a valid SPARQL Query
        /// </summary>
        /// <param name="data">Data</param>
        /// <returns></returns>
        public ISyntaxValidationResults Validate(string data)
        {
            String message;
            try
            {
                SparqlQuery q = this._parser.ParseFromString(data);
                message = "Valid SPARQL Query";

                return new SyntaxValidationResults(true, message, q);
            }
            catch (RdfParseException parseEx)
            {
                message = "Invalid SPARQL Query - Parsing Error - " + parseEx.Message;
                return new SyntaxValidationResults(message, parseEx);
            }
            catch (RdfException rdfEx)
            {
                message = "Invalid SPARQL Query - RDF Error - " + rdfEx.Message;
                return new SyntaxValidationResults(message, rdfEx);
            }
            catch (Exception ex)
            {
                message = "Invalid SPARQL Query - Error - " + ex.Message;
                return new SyntaxValidationResults(message, ex);
            }
        }
    }
}
