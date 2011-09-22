/*

Copyright Robert Vesse 2009-11
rvesse@vdesign-studios.com

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Collections.Generic;
using System.Linq;
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

        /// <summary>
        /// Constants for additional URIs provided by the Full Text Configuration Namespace
        /// </summary>
        public const String ClassIndex = FullTextConfigurationNamespace + "Index",
                            ClassIndexer = FullTextConfigurationNamespace + "Indexer",
                            ClassAnalyzer = FullTextConfigurationNamespace + "Analyzer",
                            ClassSchema = FullTextConfigurationNamespace + "Schema",
                            ClassSearcher = FullTextConfigurationNamespace + "Searcher",
                            PropertyIndexer = FullTextConfigurationNamespace + "indexer",
                            PropertyIndex = FullTextConfigurationNamespace + "index",
                            PropertyAnalyzer = FullTextConfigurationNamespace + "analyzer",
                            PropertySearcher = FullTextConfigurationNamespace + "searcher",
                            PropertySchema = FullTextConfigurationNamespace + "schema",
                            PropertyVersion = FullTextConfigurationNamespace + "version",
                            PropertyEnsureIndex = FullTextConfigurationNamespace + "ensureIndex",
                            PropertyBuildIndexFor = FullTextConfigurationNamespace + "buildIndexFor",
                            PropertyBuildIndexWith = FullTextConfigurationNamespace + "buildIndexWith";

        /// <summary>
        /// Used to extract the patterns that make up a <see cref="FullTextPattern">FullTextPattern</see>
        /// </summary>
        /// <param name="patterns">Triple Patterns</param>
        /// <returns></returns>
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

        /// <summary>
        /// Used to help extract the patterns that make up a <see cref="FullTextPattern">FullTextPattern</see>
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="subj">Subject</param>
        /// <param name="ps">Patterns</param>
        /// <param name="ftPatterns">Discovered Full Text Patterns</param>
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
