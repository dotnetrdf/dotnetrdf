using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Update;

namespace rdfEditor.Syntax
{
    public class SparqlUpdateValidator : ISyntaxValidator
    {
        private SparqlUpdateParser _parser = new SparqlUpdateParser();

        public ISyntaxValidationResults Validate(string data)
        {
            String message;
            try
            {
                SparqlUpdateCommandSet cmds = this._parser.ParseFromString(data);
                message = "Valid SPARQL Update";

                return new SyntaxValidationResults(true, message, cmds);
            }
            catch (RdfParseException parseEx)
            {
                message = "Invalid SPARQL Update - Parsing Error - " + parseEx.Message;
                return new SyntaxValidationResults(message, parseEx);
            }
            catch (RdfException rdfEx)
            {
                message = "Invalid SPARQL Update - RDF Error - " + rdfEx.Message;
                return new SyntaxValidationResults(message, rdfEx);
            }
            catch (Exception ex)
            {
                message = "Invalid SPARQL Update - Error - " + ex.Message;
                return new SyntaxValidationResults(message, ex);
            }
        }
    }
}
