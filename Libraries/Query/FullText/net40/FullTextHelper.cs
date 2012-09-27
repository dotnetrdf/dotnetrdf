/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

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
                            PropertyBuildIndexWith = FullTextConfigurationNamespace + "buildIndexWith",
                            PropertyIndexNow = FullTextConfigurationNamespace + "indexNow",
                            PropertyIndexSync = FullTextConfigurationNamespace + "indexSync";

        /// <summary>
        /// Context Key used to store and retrieve the Search Provider in the Query Evaluation Context
        /// </summary>
        public const String ContextKey = "Query.FullText.SearchProvider";
        
        }
}
