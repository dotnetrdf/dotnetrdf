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
using System.IO;
using VDS.RDF.Parsing.Handlers;

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
            _parser = parser;
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
                _parser.Load(handler, new StringReader(data));

                message = "Valid RDF Dataset - " + handler.GraphCount + " Graphs with " + handler.TripleCount + " Triples - Parser: " + _parser.GetType().Name;
                return new SyntaxValidationResults(true, message, handler);
            }
            catch (RdfParseException parseEx)
            {
                message = "Invalid RDF Dataset - Parsing Error from Parser: " + _parser.GetType().Name + " - " + parseEx.Message;
                return new SyntaxValidationResults(message, parseEx);
            }
            catch (RdfException rdfEx)
            {
                message = "Invalid RDF Dataset - RDF Error from Parser: " + _parser.GetType().Name + " - " + rdfEx.Message;
                return new SyntaxValidationResults(message, rdfEx);
            }
            catch (Exception ex)
            {
                message = "Invalid RDF Dataset - Error from Parser: " + _parser.GetType().Name + " - " + ex.Message;
                return new SyntaxValidationResults(message, ex);
            }
        }
    }
}
