using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace rdfEditor.Syntax
{
    public class RdfDatasetSyntaxValidator : ISyntaxValidator
    {
        private IStoreReader _parser;

        public RdfDatasetSyntaxValidator(IStoreReader parser)
        {
            this._parser = parser;
        }

        public virtual ISyntaxValidationResults Validate(string data)
        {
            String message;
            try
            {
                TripleStore store = new TripleStore();
                StringParser.ParseDataset(store, data, this._parser);

                message = "Valid RDF Dataset - " + store.Graphs.Count + " Graphs with " + store.Triples.Count() + " Triples - Parser: " + this._parser.GetType().Name;
                return new SyntaxValidationResults(true, message, store);
            }
            catch (RdfParseException parseEx)
            {
                message = "Invalid RDF Dataset - Parsing Error from Parser: " + this._parser.GetType().Name + " - " + parseEx.Message;
                return new SyntaxValidationResults(message, parseEx);
            }
            catch (RdfException rdfEx)
            {
                message = "Invalid RDF Dataset - RDF Error from Parser: " + this._parser.GetType().Name + " - " + rdfEx.Message;
                return new SyntaxValidationResults(message, rdfEx);
            }
            catch (Exception ex)
            {
                message = "Invalid RDF Dataset - Error from Parser: " + this._parser.GetType().Name + " - " + ex.Message;
                return new SyntaxValidationResults(message, ex);
            }
        }
    }
}
