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

namespace VDS.RDF.Parsing.Validation
{
    public class RdfSyntaxValidator : ISyntaxValidator
    {
        protected IRdfReader _parser;

        public RdfSyntaxValidator(IRdfReader parser)
        {
            this._parser = parser;
        }

        public virtual ISyntaxValidationResults Validate(string data)
        {
            String message;
            try
            {
                Graph g = new Graph(true);
                StringParser.Parse(g, data, this._parser);

                message = "Valid RDF - " + g.Triples.Count + " Triples - Parser: " + this._parser.GetType().Name;
                return new SyntaxValidationResults(true, message, g);
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

    public class RdfStrictSyntaxValidator : RdfSyntaxValidator
    {
        private bool _gotWarning = false;
        private List<String> _messages = new List<string>();

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

        public override ISyntaxValidationResults Validate(string data)
        {
            String message;
            try
            {
                this._gotWarning = false;
                this._messages.Clear();
                Graph g = new Graph(true);
                StringParser.Parse(g, data, this._parser);

                if (!this._gotWarning)
                {
                    message = "Valid RDF - " + g.Triples.Count + " Triples - Parser: " + this._parser.GetType().Name;
                    return new SyntaxValidationResults(true, message, g);
                }
                else
                {
                    message = "Valid RDF with Warnings - " + g.Triples.Count + " Triples - Parser: " + this._parser.GetType().Name + " - " + this._messages.Count + " Warnings";
                    int i = 1;
                    foreach (String m in this._messages)
                    {
                        message += "\n" + i + " - " + m;
                        i++;
                    }
                    return new SyntaxValidationResults(false, message, g, this._messages);
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
