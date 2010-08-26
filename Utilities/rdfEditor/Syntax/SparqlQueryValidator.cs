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


        public bool Validate(string data, out string message)
        {
            try
            {
                SparqlQuery q = this._parser.ParseFromString(data);
                message = "Valid SPARQL Query";

                return true;
            }
            catch (RdfParseException parseEx)
            {
                message = "Invalid SPARQL Query - Parsing Error - " + parseEx.Message;
            }
            catch (RdfException rdfEx)
            {
                message = "Invalid SPARQL Query - RDF Error - " + rdfEx.Message;
            }
            catch (Exception ex)
            {
                message = "Invalid SPARQL Query - Error - " + ex.Message;
            }
            return false;
        }
    }
}
