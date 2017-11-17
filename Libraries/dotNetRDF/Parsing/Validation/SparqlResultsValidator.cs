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
            _parser = parser;
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
                message = "Valid SPARQL Results - " + results.Count + " Results - Parser: " + _parser.GetType().Name;

                return new SyntaxValidationResults(true, message, results);
            }
            catch (RdfParseException parseEx)
            {
                message = "Invalid SPARQL Results - Parsing Error from Parser: " + _parser.GetType().Name + " - " + parseEx.Message;
                return new SyntaxValidationResults(message, parseEx);
            }
            catch (RdfException rdfEx)
            {
                message = "Invalid SPARQL Results - RDF Error from Parser: " + _parser.GetType().Name + " - " + rdfEx.Message;
                return new SyntaxValidationResults(message, rdfEx);
            }
            catch (Exception ex)
            {
                message = "Invalid SPARQL Results - Error from Parser: " + _parser.GetType().Name + " - " + ex.Message;
                return new SyntaxValidationResults(message, ex);
            }
        }
    }
}
