using System.Linq;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Algebra;

namespace VDS.RDF.Query
{
    public static class ResultsHandlerExtensions
    {
        internal static void Apply(this ISparqlResultsHandler handler, SparqlEvaluationContext context)
        {
            try
            {
                handler.StartResults();

                SparqlQuery q = context.Query;
                SparqlQueryType type;
                if (q == null)
                {
                    type = (context.OutputMultiset.Variables.Any() || context.OutputMultiset.Sets.Any() ? SparqlQueryType.Select : SparqlQueryType.Ask);
                }
                else
                {
                    type = q.QueryType;
                }

                if (type == SparqlQueryType.Ask)
                {
                    // ASK Query so get the handler to handle an appropriate boolean result
                    if (context.OutputMultiset is IdentityMultiset)
                    {
                        handler.HandleBooleanResult(true);
                    }
                    else if (context.OutputMultiset is NullMultiset)
                    {
                        handler.HandleBooleanResult(false);
                    }
                    else
                    {
                        handler.HandleBooleanResult(!context.OutputMultiset.IsEmpty);
                    }
                }
                else
                {
                    // SELECT Query so get the handler to handle variables and then handle results
                    foreach (var var in context.OutputMultiset.Variables)
                    {
                        if (!handler.HandleVariable(var)) ParserHelper.Stop();
                    }
                    foreach (ISet s in context.OutputMultiset.Sets)
                    {
                        if (!handler.HandleResult(s.AsSparqlResult())) ParserHelper.Stop();
                    }

                    // The VirtualCount property on SparqlQuery has been marked obsolete
                    // as it doesn't make sense to open it up as a public property and
                    // in any case the count of results feels like it is more properly 
                    // recorded in the results.
                    // q.VirtualCount = context.OutputMultiset.VirtualCount;

                    // TODO: Create an extended ISparqlResultsHandler that can receive the virtual count. e.g.
                    // handler.SetVirtualCount(context.OutputMultiset.VirtualCount);

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
