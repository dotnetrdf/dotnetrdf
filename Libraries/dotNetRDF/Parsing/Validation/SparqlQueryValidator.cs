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
            _parser = new SparqlQueryParser();
        }

        /// <summary>
        /// Creates a new SPARQL Query Validator using the given Syntax
        /// </summary>
        /// <param name="syntax">Query Syntax</param>
        public SparqlQueryValidator(SparqlQuerySyntax syntax)
        {
            _parser = new SparqlQueryParser(syntax);
        }

        /// <summary>
        /// Creates a new SPARQL Query Validator using the given Query Parser
        /// </summary>
        /// <param name="parser">Query Parser</param>
        public SparqlQueryValidator(SparqlQueryParser parser)
        {
            _parser = parser;
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
                SparqlQuery q = _parser.ParseFromString(data);
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
