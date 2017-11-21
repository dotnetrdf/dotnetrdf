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
                SparqlUpdateCommandSet cmds = _parser.ParseFromString(data);
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
