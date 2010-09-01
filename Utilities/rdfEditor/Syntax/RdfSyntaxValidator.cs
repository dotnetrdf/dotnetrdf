using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace rdfEditor.Syntax
{
    public class RdfSyntaxValidator : ISyntaxValidator
    {
        protected IRdfReader _parser;

        public RdfSyntaxValidator(IRdfReader parser)
        {
            this._parser = parser;
        }

        public virtual bool Validate(string data, out string message)
        {
            try
            {
                Graph g = new Graph();
                StringParser.Parse(g, data, this._parser);

                message = "Valid RDF - " + g.Triples.Count + " Triples - Parser: " + this._parser.GetType().Name;
                return true;
            }
            catch (RdfParseException parseEx)
            {
                message = "Invalid RDF - Parsing Error from Parser: " + this._parser.GetType().Name + " - " + parseEx.Message;
            }
            catch (RdfException rdfEx)
            {
                message = "Invalid RDF - RDF Error from Parser: " + this._parser.GetType().Name + " - " + rdfEx.Message;
            }
            catch (Exception ex)
            {
                message = "Invalid RDF - Error from Parser: " + this._parser.GetType().Name + " - " + ex.Message;
            }
            return false;
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

        public override bool Validate(string data, out string message)
        {
            try
            {
                this._gotWarning = false;
                this._messages.Clear();
                Graph g = new Graph();
                StringParser.Parse(g, data, this._parser);

                if (!this._gotWarning)
                {
                    message = "Valid RDF - " + g.Triples.Count + " Triples - Parser: " + this._parser.GetType().Name;
                    return true;
                }
                else
                {
                    message = "Valid RDF with Warnings - " + g.Triples.Count + " Triples - Parser: " + this._parser.GetType().Name + " - " + this._messages.Count + " Warnings";
                    foreach (String m in this._messages)
                    {
                        message += "\n" + m;
                    }
                    return false;
                }
            }
            catch (RdfParseException parseEx)
            {
                message = "Invalid RDF - Parsing Error from Parser: " + this._parser.GetType().Name + " - " + parseEx.Message;
            }
            catch (RdfException rdfEx)
            {
                message = "Invalid RDF - RDF Error from Parser: " + this._parser.GetType().Name + " - " + rdfEx.Message;
            }
            catch (Exception ex)
            {
                message = "Invalid RDF - Error from Parser: " + this._parser.GetType().Name + " - " + ex.Message;
            }
            return false;
        }
    }
}
