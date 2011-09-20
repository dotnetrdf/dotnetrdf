using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query
{
    /// <summary>
    /// Helper Class containing constants and static functions for use with Full Text Search
    /// </summary>
    public static class FullTextHelper
    {
        /// <summary>
        /// Constant for Full Text Namespace
        /// </summary>
        /// <remarks>
        /// Is actually the ARQ Property Function Namespace and is used for compatibility with ARQ syntax for full text search
        /// </remarks>
        public const String FullTextMatchNamespace = "http://jena.hpl.hp.com/ARQ/property#";

        /// <summary>
        /// Constant for Full Text Match Predicate
        /// </summary>
        public const String FullTextMatchPredicateUri = FullTextMatchNamespace + "textMatch";

        /// <summary>
        /// Constant for the Full Text Configuration Namespace which provides extensions to the basic Configuration API specific to Full Text indexing and search
        /// </summary>
        /// <remarks>
        /// This vocabulary can be found as an embedded resource in this library as <strong>VDS.RDF.Query.FullText.ttl</strong>
        /// </remarks>
        public const String FullTextConfigurationNamespace = "http://www.dotnetrdf.org/configuration/fulltext#";

        internal static List<FullTextPattern> ExtractPatterns(IEnumerable<ITriplePattern> patterns)
        {
            //Do a first pass which simply looks to find any pf:textMatch properties
            Dictionary<PatternItem, List<TriplePattern>> ftPatterns = new Dictionary<PatternItem, List<TriplePattern>>();
            List<TriplePattern> ps = patterns.OfType<TriplePattern>().ToList();
            if (ps.Count == 0) return new List<FullTextPattern>();
            foreach (TriplePattern tp in ps)
            {
                NodeMatchPattern predItem = tp.Predicate as NodeMatchPattern;
                if (predItem == null) continue;
                IUriNode predNode = predItem.Node as IUriNode;
                if (predNode == null) continue;
                if (predNode.Uri.ToString().Equals(FullTextMatchPredicateUri))
                {
                    ftPatterns.Add(tp.Subject, new List<TriplePattern>());
                    ftPatterns[tp.Subject].Add(tp);
                }
            }
            //Remove any Patterns we found from the original patterns
            foreach (List<TriplePattern> fps in ftPatterns.Values)
            {
                fps.ForEach(tp => ps.Remove(tp));
            }

            if (ftPatterns.Count == 0) return new List<FullTextPattern>();

            //Now for each pf:textMatch property we found do a further search to see if we are using
            //the (?match ?score) form or the {'text' threshold limit) form rather than the simple ?match form
            foreach (PatternItem key in ftPatterns.Keys)
            {
                if (key.VariableName != null && key.VariableName.StartsWith("_:"))
                {
                    ExtractRelatedPatterns(key, key, ps, ftPatterns);
                }
                PatternItem searchKey = ftPatterns[key].First().Object;
                if (searchKey.VariableName != null && searchKey.VariableName.StartsWith("_:"))
                {
                    ExtractRelatedPatterns(key, searchKey, ps, ftPatterns);
                }
            }

            return (from key in ftPatterns.Keys
                    select new FullTextPattern(ftPatterns[key])).ToList();
        }

        internal static void ExtractRelatedPatterns(PatternItem key, PatternItem subj, List<TriplePattern> ps, Dictionary<PatternItem, List<TriplePattern>> ftPatterns)
        {
            bool recurse = true;
            PatternItem nextSubj = null;
            foreach (TriplePattern tp in ps)
            {
                if (tp.Subject.VariableName == subj.VariableName)
                {
                    NodeMatchPattern predItem = tp.Predicate as NodeMatchPattern;
                    if (predItem == null) continue;
                    IUriNode predNode = predItem.Node as IUriNode;
                    if (predNode == null) continue;
                    if (predNode.Uri.ToString().Equals(RdfSpecsHelper.RdfListFirst))
                    {
                        ftPatterns[key].Add(tp);
                    }
                    else if (predNode.Uri.ToString().Equals(RdfSpecsHelper.RdfListRest))
                    {
                        ftPatterns[key].Add(tp);
                        recurse = tp.Object.VariableName != null;
                        nextSubj = tp.Object;
                    }
                }
            }

            ftPatterns[key].ForEach(tp => ps.Remove(tp));

            if (nextSubj == null) throw new RdfQueryException("Failed to find expected rdf:rest property");
            if (recurse)
            {
                ExtractRelatedPatterns(key, nextSubj, ps, ftPatterns);
            }
        }
    }
}
