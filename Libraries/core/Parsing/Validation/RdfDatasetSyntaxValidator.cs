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
using System.Linq;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Storage.Params;

namespace VDS.RDF.Parsing.Validation
{
    /// <summary>
    /// Syntax Validator for RDF Dataset Formats
    /// </summary>
    public class RdfDatasetSyntaxValidator : ISyntaxValidator
    {
        private IStoreReader _parser;

        /// <summary>
        /// Creates a new RDF Dataset Syntax Validator
        /// </summary>
        /// <param name="parser">Dataset Parser</param>
        public RdfDatasetSyntaxValidator(IStoreReader parser)
        {
            this._parser = parser;
        }

        /// <summary>
        /// Determines whether the data provided is valid syntax
        /// </summary>
        /// <param name="data">Data</param>
        /// <returns></returns>
        public virtual ISyntaxValidationResults Validate(string data)
        {
            String message;
            try
            {
                StoreCountHandler handler = new StoreCountHandler();
                this._parser.Load(handler, new TextReaderParams(new StringReader(data)));

                message = "Valid RDF Dataset - " + handler.GraphCount + " Graphs with " + handler.TripleCount + " Triples - Parser: " + this._parser.GetType().Name;
                return new SyntaxValidationResults(true, message, handler);
            }
            catch (RdfParseException parseEx)
            {
                message = "Invalid RDF Dataset - Parsing Error from Parser: " + this._parser.GetType().Name + " - " + parseEx.Message;
                return new SyntaxValidationResults(message, parseEx);
            }
            catch (RdfException rdfEx)
            {
                message = "Invalid RDF Dataset - RDF Error from Parser: " + this._parser.GetType().Name + " - " + rdfEx.Message;
                return new SyntaxValidationResults(message, rdfEx);
            }
            catch (Exception ex)
            {
                message = "Invalid RDF Dataset - Error from Parser: " + this._parser.GetType().Name + " - " + ex.Message;
                return new SyntaxValidationResults(message, ex);
            }
        }
    }
}
