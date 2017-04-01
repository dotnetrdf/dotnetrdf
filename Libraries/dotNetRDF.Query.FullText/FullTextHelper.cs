/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
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
