using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Results;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF
{
    public static class QueryTestTools
    {
        private static readonly INodeFormatter _formatter = new NTriplesFormatter();

        public static void ShowResults(IQueryResult result)
        {
            if (result.IsBoolean)
            {
                Console.WriteLine("Boolean Result = " + result.Boolean.Value.ToString());
                return;
            }
            if (result.IsGraph)
            {
                TestTools.ShowGraph(result.Graph);
                return;
            }
            ShowResults(result.Table);
        }

        public static void ShowResults(ITabularResults results)
        {
            foreach (IResultRow row in results)
            {
                Console.WriteLine(row.ToString(_formatter));
            }
        }
    }
}
