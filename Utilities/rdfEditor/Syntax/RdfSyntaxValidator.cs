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
        private IRdfReader _parser;

        public RdfSyntaxValidator(IRdfReader parser)
        {
            this._parser = parser;
        }

        public bool Validate(string data, out string message)
        {
            try
            {
                Graph g = new Graph();
                StringParser.Parse(g, data, this._parser);

                message = "Valid RDF - " + g.Triples.Count + " Triples - Parser " + this._parser.GetType().Name;
                return true;
            }
            catch (RdfParseException parseEx)
            {
                message = "Invalid RDF - Parsing Error from Parser " + this._parser.GetType().Name + " - " + parseEx.Message;
            }
            catch (RdfException rdfEx)
            {
                message = "Invalid RDF - RDF Error from Parser " + this._parser.GetType().Name + " - " + rdfEx.Message;
            }
            catch (Exception ex)
            {
                message = "Invalid RDF - Error from Parser " + this._parser.GetType().Name + " - " + ex.Message;
            }
            return false;
        }
    }
}
