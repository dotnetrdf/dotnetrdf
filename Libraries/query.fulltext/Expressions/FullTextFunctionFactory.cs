using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Query.Expressions
{
    public class FullTextFunctionFactory
        : ISparqlCustomExpressionFactory
    {

        public bool TryCreateExpression(Uri u, List<ISparqlExpression> args, Dictionary<string, ISparqlExpression> scalarArguments, out ISparqlExpression expr)
        {
            //TODO: Add support for FullTextMatchFunction and FullTextSearchFunction

            //switch (u.ToString())
            //{

            //}

            expr = null;
            return false;
        }

        public IEnumerable<Uri> AvailableExtensionFunctions
        {
            get 
            {
                return Enumerable.Empty<Uri>(); 
            }
        }

        public IEnumerable<Uri> AvailableExtensionAggregates
        {
            get 
            {
                return Enumerable.Empty<Uri>();
            }
        }
    }
}
