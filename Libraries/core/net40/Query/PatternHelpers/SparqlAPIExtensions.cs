using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing.Tokens;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query
{
    /// <summary>
    /// A set of extension methods to help work with TriplePattern and GraphPattern instances
    /// </summary>
    /// <remarks>
    /// TODO check when cloning is truly required
    /// </remarks>
    public static class SparqlAPIExtensions
    {

        /// <summary>
        /// Returns a shallow or full clone of a GraphPattern instance
        /// </summary>
        /// <param name="source"></param>
        /// <param name="inDepth"></param>
        /// <returns></returns>
        public static GraphPattern Clone(this GraphPattern source, bool shallow = false) {
            return new GraphPattern(source, shallow);
        }

        /// <summary>
        /// Returns a MinusGraphPattern that wraps a copy of the given pattern
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static GraphPattern WithinMinus(this GraphPattern source)
        {
            GraphPattern gp = new GraphPattern();
            gp.IsMinus = true;
            gp.AddGraphPattern(source.Clone());
            return gp;
        }

        /// <summary>
        /// Returns a OptionalGraphPattern that wraps a copy of the given pattern
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static GraphPattern WithinOptional(this GraphPattern source)
        {
            GraphPattern gp = new GraphPattern();
            gp.IsOptional = true;
            gp.AddGraphPattern(source.Clone());
            return gp;
        }

        /// <summary>
        /// Returns a UnionGraphPattern from the given list of GraphPatterns copies
        /// </summary>
        /// <param name="sources"></param>
        /// <returns></returns>
        public static GraphPattern ToUnionGraphPattern(this IEnumerable<GraphPattern> sources)
        {
            GraphPattern gp = new GraphPattern();
            gp.IsUnion = true;
            foreach (GraphPattern source in sources) gp.AddGraphPattern(source.Clone());
            return gp;
        }

        /// <summary>
        /// Returns a GraphGraphPattern that wraps a copy of the given pattern
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static GraphPattern WithinGraph(this GraphPattern source, IToken token)
        {
            GraphPattern gp;
            if (source.IsService) return source; // is this correct ?
            if (source.IsGraph)
            { // cheange the graph Uri for the pattern

                gp = new GraphPattern(source);
            }
            else
            { // wraps the pattern into a GraphGraphPattern
                gp = new GraphPattern();
                gp.AddGraphPattern(source.Clone());
            }
            gp.IsGraph = true;
            gp.GraphSpecifier = token;
            return gp;
        }

        /// <summary>
        /// Returns a GraphGraphPattern that wraps a copy of the given pattern
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static GraphPattern WithinGraph(this GraphPattern source, String varName)
        {
            return WithinGraph(source, new VariableToken("?" + varName, 0, 0, 0));
        }

        /// <summary>
        /// Returns a GraphGraphPattern that wraps a copy of the given pattern
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static GraphPattern WithinGraph(this GraphPattern source, Uri graphUri)
        {
            return WithinGraph(source, new UriToken("<" + graphUri.ToString() + ">", 0, 0, 0));
        }

        /// <summary>
        /// Returns a ServiceGraphPattern that wraps a copy of the given pattern
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static GraphPattern WithinService(Uri serviceUri, GraphPattern source)
        {
            GraphPattern gp = new GraphPattern();
            gp.IsService = true;
            gp.GraphSpecifier = new UriToken("<" + serviceUri.ToString() + ">", 0, 0, 0);
            gp.AddGraphPattern(source.Clone());
            return gp;
        }

        /// <summary>
        /// Returns a GraphPattern that contains the single TriplePattern
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static GraphPattern AsGraphPattern(this ITriplePattern tp) {
            GraphPattern gp = new GraphPattern();
            gp.AddTriplePattern(tp);
            return gp;
        }
    }
}
