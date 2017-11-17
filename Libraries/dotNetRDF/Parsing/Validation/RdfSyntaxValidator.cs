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
using System.Collections.Generic;
using System.IO;
using VDS.RDF.Parsing.Handlers;

namespace VDS.RDF.Parsing.Validation
{
    /// <summary>
    /// Syntax Validator for validating RDF Graph syntaxes
    /// </summary>
    public class RdfSyntaxValidator : ISyntaxValidator
    {
        /// <summary>
        /// Parser to use
        /// </summary>
        protected IRdfReader _parser;

        /// <summary>
        /// Creates a new RDF Syntax Validator using the given Parser
        /// </summary>
        /// <param name="parser">Parser</param>
        public RdfSyntaxValidator(IRdfReader parser)
        {
            _parser = parser;
        }

        /// <summary>
        /// Validates the given data to see if it is valid RDF Syntax
        /// </summary>
        /// <param name="data">Data</param>
        /// <returns></returns>
        public virtual ISyntaxValidationResults Validate(string data)
        {
            String message;
            try
            {
                CountHandler handler = new CountHandler();
                _parser.Load(handler, new StringReader(data));

                message = "Valid RDF - " + handler.Count + " Triples - Parser: " + _parser.GetType().Name;
                return new SyntaxValidationResults(true, message, handler);
            }
            catch (RdfParseException parseEx)
            {
                message = "Invalid RDF - Parsing Error from Parser: " + _parser.GetType().Name + " - " + parseEx.Message;
                return new SyntaxValidationResults(message, parseEx);
            }
            catch (RdfException rdfEx)
            {
                message = "Invalid RDF - RDF Error from Parser: " + _parser.GetType().Name + " - " + rdfEx.Message;
                return new SyntaxValidationResults(message, rdfEx);
            }
            catch (Exception ex)
            {
                message = "Invalid RDF - Error from Parser: " + _parser.GetType().Name + " - " + ex.Message;
                return new SyntaxValidationResults(message, ex);
            }
        }
    }

    /// <summary>
    /// Syntax Validator for RDF Graph syntaxes which is strict (any warnings are treated as errors)
    /// </summary>
    public class RdfStrictSyntaxValidator : RdfSyntaxValidator
    {
        private bool _gotWarning = false;
        private List<String> _messages = new List<string>();

        /// <summary>
        /// Creates a new Strict RDF Syntax Validator
        /// </summary>
        /// <param name="parser">Parser</param>
        public RdfStrictSyntaxValidator(IRdfReader parser)
            : base(parser)
        {
            parser.Warning += OnWarning;
        }

        private void OnWarning(String message)
        {
            _gotWarning = true;
            _messages.Add(message);
        }

        /// <summary>
        /// Validates the data to see if it is valid RDF syntax which does not produce any warnings
        /// </summary>
        /// <param name="data">Data</param>
        /// <returns></returns>
        public override ISyntaxValidationResults Validate(string data)
        {
            String message;
            try
            {
                _gotWarning = false;
                _messages.Clear();
                CountHandler handler = new CountHandler();
                _parser.Load(handler, new StringReader(data));

                if (!_gotWarning)
                {
                    message = "Valid RDF - " + handler.Count + " Triples - Parser: " + _parser.GetType().Name;
                    return new SyntaxValidationResults(true, message, handler);
                }
                else
                {
                    message = "Valid RDF with Warnings - " + handler.Count + " Triples - Parser: " + _parser.GetType().Name + " - " + _messages.Count + " Warnings";
                    int i = 1;
                    foreach (String m in _messages)
                    {
                        message += "\n" + i + " - " + m;
                        i++;
                    }
                    return new SyntaxValidationResults(false, message, handler, _messages);
                }
            }
            catch (RdfParseException parseEx)
            {
                message = "Invalid RDF - Parsing Error from Parser: " + _parser.GetType().Name + " - " + parseEx.Message;
                return new SyntaxValidationResults(message, parseEx);
            }
            catch (RdfException rdfEx)
            {
                message = "Invalid RDF - RDF Error from Parser: " + _parser.GetType().Name + " - " + rdfEx.Message;
                return new SyntaxValidationResults(message, rdfEx);
            }
            catch (Exception ex)
            {
                message = "Invalid RDF - Error from Parser: " + _parser.GetType().Name + " - " + ex.Message;
                return new SyntaxValidationResults(message, ex);
            }
        }
    }
}
