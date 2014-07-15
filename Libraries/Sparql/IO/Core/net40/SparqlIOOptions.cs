using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace net40
{
    public class SparqlIOOptions
    {
        private static SparqlQuerySyntax _queryDefaultSyntax = SparqlQuerySyntax.Sparql_1_1;

        /// <summary>
        /// Gets/Sets the default syntax used for parsing SPARQL queries
        /// </summary>
        /// <remarks>
        /// The default is SPARQL 1.1 unless you use this property to change it
        /// </remarks>
        public static SparqlQuerySyntax QueryDefaultSyntax
        {
            get
            {
                return _queryDefaultSyntax;
            }
            set
            {
                _queryDefaultSyntax = value;
            }
        }
    }
}
