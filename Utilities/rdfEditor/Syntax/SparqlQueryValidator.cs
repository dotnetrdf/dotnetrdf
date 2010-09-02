using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace rdfEditor.Syntax
{
    public class SparqlQueryValidator : ISyntaxValidator
    {
        private SparqlQueryParser _parser;

        public SparqlQueryValidator()
        {
            this._parser = new SparqlQueryParser();
        }

        public SparqlQueryValidator(SparqlQuerySyntax syntax)
        {
            this._parser = new SparqlQueryParser(syntax);
        }

        public SparqlQueryValidator(SparqlQueryParser parser)
        {
            this._parser = parser;
        }


        public ISyntaxValidationResults Validate(string data)
        {
            String message;
            try
            {
                SparqlQuery q = this._parser.ParseFromString(data);
                message = "Valid SPARQL Query";

                return new SyntaxValidationResults(true, message, q);
            }
            catch (RdfParseException parseEx)
            {
                message = "Invalid SPARQL Query - Parsing Error - " + parseEx.Message;
                return new SyntaxValidationResults(message, parseEx);
            }
            catch (RdfException rdfEx)
            {
                message = "Invalid SPARQL Query - RDF Error - " + rdfEx.Message;
                return new SyntaxValidationResults(message, rdfEx);
            }
            catch (Exception ex)
            {
                message = "Invalid SPARQL Query - Error - " + ex.Message;
                return new SyntaxValidationResults(message, ex);
            }
        }
    }
}
