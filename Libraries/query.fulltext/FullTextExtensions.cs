using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.FullText.Search;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query
{
    static class FullTextExtensions
    {
        private static NodeFactory _factory = new NodeFactory();

        internal static ISet ToSet(this IFullTextSearchResult result, String matchVar, String scoreVar)
        {
            Set s = new Set();
            if (matchVar != null) s.Add(matchVar, result.Node);
            if (scoreVar != null) s.Add(scoreVar, result.Score.ToLiteral(_factory));
            return s;
        }
    }
}
