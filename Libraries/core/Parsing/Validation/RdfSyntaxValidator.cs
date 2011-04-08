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
            this._parser = parser;
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
                this._parser.Load(handler, new StringReader(data));

                message = "Valid RDF - " + handler.Count + " Triples - Parser: " + this._parser.GetType().Name;
                return new SyntaxValidationResults(true, message, handler);
            }
            catch (RdfParseException parseEx)
            {
                message = "Invalid RDF - Parsing Error from Parser: " + this._parser.GetType().Name + " - " + parseEx.Message;
                return new SyntaxValidationResults(message, parseEx);
            }
            catch (RdfException rdfEx)
            {
                message = "Invalid RDF - RDF Error from Parser: " + this._parser.GetType().Name + " - " + rdfEx.Message;
                return new SyntaxValidationResults(message, rdfEx);
            }
            catch (Exception ex)
            {
                message = "Invalid RDF - Error from Parser: " + this._parser.GetType().Name + " - " + ex.Message;
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
            parser.Warning += this.OnWarning;
        }

        private void OnWarning(String message)
        {
            this._gotWarning = true;
            this._messages.Add(message);
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
                this._gotWarning = false;
                this._messages.Clear();
                CountHandler handler = new CountHandler();
                this._parser.Load(handler, new StringReader(data));

                if (!this._gotWarning)
                {
                    message = "Valid RDF - " + handler.Count + " Triples - Parser: " + this._parser.GetType().Name;
                    return new SyntaxValidationResults(true, message, handler);
                }
                else
                {
                    message = "Valid RDF with Warnings - " + handler.Count + " Triples - Parser: " + this._parser.GetType().Name + " - " + this._messages.Count + " Warnings";
                    int i = 1;
                    foreach (String m in this._messages)
                    {
                        message += "\n" + i + " - " + m;
                        i++;
                    }
                    return new SyntaxValidationResults(false, message, handler, this._messages);
                }
            }
            catch (RdfParseException parseEx)
            {
                message = "Invalid RDF - Parsing Error from Parser: " + this._parser.GetType().Name + " - " + parseEx.Message;
                return new SyntaxValidationResults(message, parseEx);
            }
            catch (RdfException rdfEx)
            {
                message = "Invalid RDF - RDF Error from Parser: " + this._parser.GetType().Name + " - " + rdfEx.Message;
                return new SyntaxValidationResults(message, rdfEx);
            }
            catch (Exception ex)
            {
                message = "Invalid RDF - Error from Parser: " + this._parser.GetType().Name + " - " + ex.Message;
                return new SyntaxValidationResults(message, ex);
            }
        }
    }
}
