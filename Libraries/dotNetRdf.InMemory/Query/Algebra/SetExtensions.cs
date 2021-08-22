using System.Collections.Generic;
using System.Linq;

namespace VDS.RDF.Query.Algebra
{
    /// <summary>
    /// Additional utility methods for ISet
    /// </summary>
    public static class SetExtensions
    {
        /// <summary>
        /// Creates a <see cref="SparqlResult"/> instance that contains all of the variables in this set.
        /// </summary>
        /// <param name="set"></param>
        /// <returns></returns>
        public static SparqlResult AsSparqlResult(this ISet set)
        {
            return new SparqlResult(set.Variables.Select(var => new KeyValuePair<string, INode>(var, set[var])));
        }

        /// <summary>
        /// Creates a <see cref="SparqlResult"/> instance that contains the bindings for the specified variables in the set.
        /// </summary>
        /// <param name="set">The set containing the bindings to be added to the SPARQL result.</param>
        /// <param name="variables">The names of the variables to be included in the SPARQL result.</param>
        /// <returns></returns>
        public static SparqlResult AsSparqlResult(this ISet set, IEnumerable<string> variables)
        {
            return new SparqlResult(variables.Where(set.ContainsVariable)
                .Select(x => new KeyValuePair<string, INode>(x, set[x])));
        }
    }
}
