/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2013 dotNetRDF Project (dotnetrdf-develop@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager.Tasks
{
    // TODO Needs a massive code clean up
    // - Should not use string manipulation to build originalQuery

    /// <summary>
    /// A task that takes a query and runs it using the results of that query to build a new query
    /// </summary>
    public class GenerateEntitiesQueryTask
        : NonCancellableTask<String>
    {
        private readonly IQueryableStorage _storage;
        private readonly SparqlQueryParser _parser = new SparqlQueryParser(SparqlQuerySyntax.Extended);
        private readonly INodeFactory _nodeFactory = new NodeFactory();
        private readonly int _minValuesPerPredicateLimit;
        private readonly int _columnNameWords;

        /// <summary>
        /// Creates a new task
        /// </summary>
        /// <param name="storage">Storage Provider</param>
        /// <param name="originalQuery">Query String</param>
        /// <param name="minValuesPerPredicateLimit">Minimum values per predicate limit</param>
        /// <param name="columnNameWords"></param>
        public GenerateEntitiesQueryTask(IQueryableStorage storage, String originalQuery, int minValuesPerPredicateLimit, int columnNameWords)
            : base("Generate Entities Query")
        {
            this._storage = storage;
            this.OriginalQueryString = originalQuery;
            this._minValuesPerPredicateLimit = minValuesPerPredicateLimit;
            this._columnNameWords = columnNameWords;
        }

        /// <summary>
        /// Gets the original query string that the user provided
        /// </summary>
        public string OriginalQueryString { get; private set; }

        protected override String RunTaskInternal()
        {
            // Start from an initial Original Query issued by user
            SparqlQuery initialQuery;
            try
            {
                initialQuery = this._parser.ParseFromString(this.OriginalQueryString);
            }
            catch (RdfParseException parseEx)
            {
                throw new RdfQueryException("Unable to parse the given SPARQL Query", parseEx);
            }

            if (initialQuery.QueryType != SparqlQueryType.Select && initialQuery.QueryType != SparqlQueryType.SelectDistinct)
            {
                throw new RdfQueryException("Only Sparql select Query with variables is supported for table format");
            }

            INamespaceMapper originalPrefixes = new NamespaceMapper(true);
            originalPrefixes.Import(initialQuery.NamespaceMap);
            initialQuery.NamespaceMap.Clear();

            // Get the predicates for first variable (entity subjects)
            // SELECT DISTINCT ?p (COUNT(?p) as ?count)
            // WHERE
            // {
            //   ?sx ?p ?o.
            //   {
            //     # Original Query
            //   }
            // }
            // GROUP BY ?p
            // Create a dynamic originalQuery

            SparqlVariable subject = initialQuery.Variables.FirstOrDefault(v => v.IsResultVariable);
            if (subject == null) throw new RdfQueryException("At least one result variable is required to generate an entities query");
            INode subjectNode = this._nodeFactory.CreateVariableNode(subject.Name);

            SparqlParameterizedString getPredicatesQuery = new SparqlParameterizedString();
            getPredicatesQuery.Namespaces = originalPrefixes;
            getPredicatesQuery.CommandText = @"
SELECT DISTINCT ?p (COUNT(?p) as ?count)
WHERE
{
  @subject ?p ?o.
";
            getPredicatesQuery.AppendSubQuery(initialQuery);
            getPredicatesQuery.CommandText += @"}
GROUP BY ?p";
            getPredicatesQuery.SetParameter("subject", subjectNode);

            try
            {
                // Get predicates and number of usages
#if DEBUG
                Debug.WriteLine(getPredicatesQuery.ToString());
#endif
                SparqlResultSet sparqlResults = (SparqlResultSet) this._storage.Query(getPredicatesQuery.ToString());
                if (sparqlResults == null)
                {
                    throw new RdfQueryException("Unexpected results type received while trying to build entities query");
                }

                List<String> selectColumns = new List<String>();
                selectColumns.Add(subject.Name);
                SparqlParameterizedString entitiesQuery = new SparqlParameterizedString();
                entitiesQuery.Namespaces.Import(getPredicatesQuery.Namespaces);
                entitiesQuery.SetParameter("subject", subjectNode);

                // For each predicate add a column and an appropriate OPTIONAL clause
                int predicateIndex = 0;
                StringBuilder optionalFilters = new StringBuilder();
                foreach (var sparqlResult in sparqlResults)
                {
                    INode predicate = sparqlResult["p"];
                    IValuedNode count = sparqlResult["count"].AsValuedNode();
                    long predicateCount = count != null ? count.AsInteger() : 0;

                    // For each predicate with predicateCount > _minValuesPerPredicateLimit add a new column and an optional filter in where clause
                    if (predicateCount <= _minValuesPerPredicateLimit) continue;
                    predicateIndex++;
                    String predicateColumnName = GetColumnName(predicate) + predicateIndex;
                    selectColumns.Add(predicateColumnName);

                    optionalFilters.AppendLine("  OPTIONAL { @subject @predicate" + predicateIndex + " ?" + predicateColumnName + " }");
                    entitiesQuery.SetParameter("predicate" + predicateIndex, predicate);
                }

                entitiesQuery.CommandText = "SELECT DISTINCT ";
                foreach (String column in selectColumns)
                {
                    entitiesQuery.CommandText += "?" + column + " ";
                }
                entitiesQuery.CommandText += @"
WHERE
{
";
                entitiesQuery.AppendSubQuery(initialQuery);
                entitiesQuery.CommandText += optionalFilters.ToString();
                entitiesQuery.CommandText += @"
}";

                this.OutputTableQuery = entitiesQuery.ToString();
            }
            catch (Exception)
            {
                this.OutputTableQuery = null;
                throw;
            }

            return this.OutputTableQuery;
        }

        /// <summary>
        /// Get column name by replacing special chars with '_'
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private string GetColumnName(INode node)
        {
            // TODO A / would be invalid in a SPARQL variable name so escape it
            // TODO Use the predicates from the original query to create a compact column name when possible
            // TODO Access the URI sensibly rather than by ugly string manipulation
            var nodeString = node.ToString();
            if (nodeString.StartsWith("<"))
            {
                nodeString = nodeString.Substring(1, nodeString.Length - 2); //remove <>
            }

            // For a given uri: "http://www.example.org/word1/word2" and _columnNameWords = 2 the result is: word1/word2
            // For a given uri: "http://www.example.org/word1/word2" and _columnNameWords = 1 the result is: word1
            int startIndex = 0;
            var wordsCount = 0;
            for (int i = nodeString.Length - 1; i >= 0; i--)
            {
                if (nodeString[i] != '/' && nodeString[i] != '#') continue;
                wordsCount++;
                if (wordsCount != _columnNameWords) continue;
                startIndex = i;
                break;
            }
            nodeString = nodeString.Substring(startIndex + 1, nodeString.Length - startIndex - 1);

            // Replace special characters
            var validPredicate = Regex.Replace(nodeString, @"[^\d\w\s]", "_");
            return validPredicate;
        }

        /// <summary>
        /// Gets/Sets the output table originalQuery
        /// </summary>
        public string OutputTableQuery { get; private set; }
    }
}