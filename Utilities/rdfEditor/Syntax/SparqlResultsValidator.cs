using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace rdfEditor.Syntax
{
    public class SparqlResultsValidator : ISyntaxValidator
    {
        private ISparqlResultsReader _parser;

        public SparqlResultsValidator(ISparqlResultsReader parser)
        {
            this._parser = parser;
        }

        public bool Validate(string data, out string message)
        {
            try
            {
                SparqlResultSet results = new SparqlResultSet();
                StringParser.ParseResultSet(results, data);
                message = "Valid SPARQL Results - " + results.Count + " Results - Parser: " + this._parser.GetType().Name;

                return true;
            }
            catch (RdfParseException parseEx)
            {
                message = "Invalid SPARQL Results - Parsing Error from Parser: " + this._parser.GetType().Name + " - " + parseEx.Message;
            }
            catch (RdfException rdfEx)
            {
                message = "Invalid SPARQL Results - RDF Error from Parser: " + this._parser.GetType().Name + " - " + rdfEx.Message;
            }
            catch (Exception ex)
            {
                message = "Invalid SPARQL Results - Error from Parser: " + this._parser.GetType().Name + " - " + ex.Message;
            }
            return false;
        }
    }
}
