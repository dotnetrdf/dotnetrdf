using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Query.Expressions
{
    public class FullTextFunctionFactory
        : ISparqlCustomExpressionFactory
    {
        private const String TextMatchFunctionUri = "http://jena.hpl.hp.com/ARQ/property#textMatch";

        public bool TryCreateExpression(Uri u, List<ISparqlExpression> args, Dictionary<string, ISparqlExpression> scalarArguments, out ISparqlExpression expr)
        {
            switch (u.ToString())
            {
                case TextMatchFunctionUri:
                    //TODO: Implement returning a TextMatch Function
                    break;
            }

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
