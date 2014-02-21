using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query;

namespace VDS.RDF.Parsing.Handlers
{
    public static class SparqlHandlerExtensions
    {
        /// <summary>
        /// Applies the result set to a Results Handler
        /// </summary>
        /// <param name="handler">Results Handler</param>
        /// <param name="results">Result Set</param>
        public static void Apply(this ISparqlResultsHandler handler, SparqlResultSet results)
        {
            try
            {
                handler.StartResults();

                switch (results.ResultsType)
                {
                    case SparqlResultsType.Boolean:
                        handler.HandleBooleanResult(results.Result);
                        break;
                    case SparqlResultsType.VariableBindings:
                        foreach (String var in results.Variables)
                        {
                            if (!handler.HandleVariable(var)) ParserHelper.Stop();
                        }
                        foreach (SparqlResult r in results)
                        {
                            if (!handler.HandleResult(r)) ParserHelper.Stop();
                        }
                        break;
                    default:
                        //Does nothing     
                        break;
                }

                handler.EndResults(true);
            }
            catch (RdfParsingTerminatedException)
            {
                handler.EndResults(true);
            }
            catch
            {
                handler.EndResults(false);
                throw;
            }
        }
    }
}
