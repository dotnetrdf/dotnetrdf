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
using VDS.RDF.Update;

namespace VDS.RDF.Parsing.Validation
{
    /// <summary>
    /// A Syntax Validator for validating SPARQL Update Commands
    /// </summary>
    public class SparqlUpdateValidator : ISyntaxValidator
    {
        private SparqlUpdateParser _parser = new SparqlUpdateParser();

        /// <summary>
        /// Validates whether the given data is a SPARQL Update Command
        /// </summary>
        /// <param name="data">Data</param>
        /// <returns></returns>
        public ISyntaxValidationResults Validate(string data)
        {
            String message;
            try
            {
                SparqlUpdateCommandSet cmds = this._parser.ParseFromString(data);
                message = "Valid SPARQL Update";

                return new SyntaxValidationResults(true, message, cmds);
            }
            catch (RdfParseException parseEx)
            {
                message = "Invalid SPARQL Update - Parsing Error - " + parseEx.Message;
                return new SyntaxValidationResults(message, parseEx);
            }
            catch (RdfException rdfEx)
            {
                message = "Invalid SPARQL Update - RDF Error - " + rdfEx.Message;
                return new SyntaxValidationResults(message, rdfEx);
            }
            catch (Exception ex)
            {
                message = "Invalid SPARQL Update - Error - " + ex.Message;
                return new SyntaxValidationResults(message, ex);
            }
        }
    }
}
