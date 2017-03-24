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
        private int _nextTempID = 0, _nextTempColumnID = -1;

        /// <summary>
        /// Creates a new task
        /// </summary>
        /// <param name="storage">Storage Provider</param>
        /// <param name="originalQuery">Query String</param>
        /// <param name="minValuesPerPredicateLimit">Minimum values per predicate limit</param>
        public GenerateEntitiesQueryTask(IQueryableStorage storage, String originalQuery, int minValuesPerPredicateLimit)
            : base("Generate Entities Query")
        {
            this._storage = storage;
            this.OriginalQueryString = originalQuery;
            this._minValuesPerPredicateLimit = minValuesPerPredicateLimit;
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
            //   {
            //     # Original Query
            //   }
            //   ?sx ?p ?o.
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
{";
            getPredicatesQuery.AppendSubQuery(initialQuery);
            getPredicatesQuery.CommandText += @"
  @subject ?p ?o.
}
GROUP BY ?p";
            getPredicatesQuery.SetParameter("subject", subjectNode);

            try
            {
                // Get predicates and number of usages
                this.Information = "Running query to extract predicates for entities selected by original query...";
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
                this.Information = "Generating Entities Query...";
                foreach (SparqlResult sparqlResult in sparqlResults)
                {
                    if (!sparqlResult.HasBoundValue("p")) continue;
                    INode predicate = sparqlResult["p"];
                    IValuedNode count = sparqlResult["count"].AsValuedNode();
                    long predicateCount = count != null ? count.AsInteger() : 0;

                    // For each predicate with predicateCount > _minValuesPerPredicateLimit add a new column and an optional filter in where clause
                    if (predicateCount <= _minValuesPerPredicateLimit) continue;
                    predicateIndex++;
                    String predicateColumnName = GetColumnName(predicate, entitiesQuery.Namespaces);
                    if (predicateColumnName == null) continue;
                    selectColumns.Add(predicateColumnName);

                    optionalFilters.AppendLine("  OPTIONAL { @subject @predicate" + predicateIndex + " ?" + predicateColumnName + " }");
                    entitiesQuery.SetParameter("predicate" + predicateIndex, predicate);
                }

                if (selectColumns.Count == 1) throw new RdfQueryException("No predicates which matched the criteria were found so an entities query cannot be generated");

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

                this.Information = "Generated Entities Query with " + (selectColumns.Count - 1) + " Predicate Columns";
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
        /// Get column name by using qname compression and replacing special chars with underscores
        /// </summary>
        /// <param name="node">Node</param>
        /// <param name="namespaces">Namespaces</param>
        /// <returns></returns>
        private string GetColumnName(INode node, INamespaceMapper namespaces)
        {
            if (node.NodeType != NodeType.Uri) return null;
            Uri u = ((IUriNode) node).Uri;

            String qname;
            if (!namespaces.ReduceToQName(u.AbsoluteUri, out qname))
            {
                // Issue temporary namespaces
                String tempNsPrefix = "ns" + this._nextTempID;
                String nsUri = u.AbsoluteUri;
                if (nsUri.Contains("#"))
                {
                    //Create a Hash Namespace Uri
                    nsUri = nsUri.Substring(0, nsUri.LastIndexOf("#") + 1);
                }
                else if (nsUri.Contains("/"))
                {
                    //Create a Slash Namespace Uri
                    nsUri = nsUri.Substring(0, nsUri.LastIndexOf("/") + 1);
                }
                Uri tempNsUri;
                if (!Uri.TryCreate(nsUri, UriKind.Absolute, out tempNsUri)) tempNsUri = u;
                while (namespaces.HasNamespace(tempNsPrefix))
                {
                    this._nextTempID++;
                    tempNsPrefix = "ns" + this._nextTempID;
                }
                namespaces.AddNamespace(tempNsPrefix, tempNsUri);
                if (!namespaces.ReduceToQName(u.AbsoluteUri, out qname))
                {
                    qname = null;
                }
            }
            if (qname == null)
            {
                // Issue temporary column name
                return "entityPredicateColumn" + (++this._nextTempColumnID);
            }
            // Sanitize qname column name
            qname = qname.StartsWith("_") ? qname.Substring(1) : qname.Replace(':', '_');
            if (SparqlSpecsHelper.IsValidVarName(qname)) return qname;
            if (qname.Length == 0)
            {
                // Issue temporary column name
                return "entityPredicateColumn " + (++this._nextTempColumnID);
            }

            // Clean up illegal characters
            // First Character must be from PN_CHARS_U or a digit
            // Add a leading p for invalid first characters
            char[] cs = qname.ToCharArray();
            char first = cs[0];
            if (!Char.IsDigit(first) || !SparqlSpecsHelper.IsPNCharsU(first))
            {
                qname = "p" + qname;
                if (SparqlSpecsHelper.IsValidVarName(qname)) return qname;
                cs = qname.ToCharArray();
            }
            for (int i = 1; i < cs.Length; i++)
            {
                if (i >= cs.Length - 1) continue;
                // Subsequent Chars must be from PN_CHARS (except -) or a '.'
                // Replace invalid characters with _
                if (cs[i] == '.' || cs[i] == '-')
                {
                    cs[i] = '_';
                }
                else if (!SparqlSpecsHelper.IsPNChars(cs[i]))
                {
                    cs[i] = '_';
                }
            }
            // Trim trailing underscores
            return new string(cs).TrimEnd('_');
        }

        /// <summary>
        /// Gets/Sets the output table originalQuery
        /// </summary>
        public string OutputTableQuery { get; private set; }
    }
}