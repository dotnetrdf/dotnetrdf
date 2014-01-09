using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using VDS.RDF.Nodes;
using VDS.RDF.Query;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager.Tasks
{
    // TODO Needs a massive code clean up
    // - Should not use string manipulation to build query
    // - Should not extend QueryTask since it does not return results rather returns a new query

    public class QueryAsTableTask
        : QueryTask
    {
        private readonly int _minValuesPerPredicateLimit;
        private readonly int _columnNameWords;

        public QueryAsTableTask(IQueryableStorage manager, string query, int minValuesPerPredicateLimit, int columnNameWords)
            : base(manager, query)
        {
            _minValuesPerPredicateLimit = minValuesPerPredicateLimit;
            _columnNameWords = columnNameWords;
        }

        public QueryAsTableTask(IQueryableStorage manager, string query, int pageSize, int minValuesPerPredicateLimit, int columnNameWords)
            : base(manager, query, pageSize)
        {
            _minValuesPerPredicateLimit = minValuesPerPredicateLimit;
        }

        protected override object RunTaskInternal()
        {
            //start from an initial query issued by user
//prefixes
//select distinct ?sx ?p ?o
//where { 
//  ?sx ?p ?o .
//}
            //get the predicates for first variable (entity subjects)

//select distinct ?p (COUNT(?p) as ?count)
//where
//{
//?sx ?p ?o.
//{
// #user query
//}
//}
//GROUP BY ?p
            //create a dynamic query
            //for each predicate with predicateCount > _minValuesPerPredicateLimit add a new column and an optional filter in where clause

            SparqlQuery intialQuery = this._parser.ParseFromString(this.QueryString);

            if (intialQuery.QueryType != SparqlQueryType.Select && intialQuery.QueryType != SparqlQueryType.SelectDistinct)
            {
                throw new RdfQueryException("Only Sparql select Query with variables is supported for table format");
            }

            // Separate prefixes from query command (since it's a select q type search for "select" keyword)
            var indexOfSelect = this.QueryString.ToLower().IndexOf("select", System.StringComparison.Ordinal);
            var intialQueryCommand = this.QueryString.Remove(0, indexOfSelect);
            SparqlVariable subject = intialQuery.Variables.First();

            var getPredicatesQueryParameterizedString = new SparqlParameterizedString();
            getPredicatesQueryParameterizedString.Namespaces = intialQuery.NamespaceMap;
            getPredicatesQueryParameterizedString.CommandText = string.Format(@"
select distinct ?p (COUNT(?p) as ?count)
where
{{
@subject ?p ?o.
{{
{0}
}}
}}
GROUP BY ?p
", intialQueryCommand);
            getPredicatesQueryParameterizedString.SetParameter("subject", new Graph().CreateVariableNode(subject.Name));

            this.QueryString = getPredicatesQueryParameterizedString.ToString();

            //get predicates and nr of usages
            var results = base.RunTaskInternal();

            //contruct second query for final result
            try
            {
                var sparqlResults = results as SparqlResultSet;

                if (sparqlResults == null)
                {
                    throw new RdfQueryException("Only SparqlQuery is supported for table format");
                }

                var selectColumns = new StringBuilder();
                selectColumns.AppendLine(subject.ToString());

                var optionalFilters = new StringBuilder();

                //foreach predicate add a column and an "optional" query clause
                foreach (var sparqlResult in sparqlResults)
                {
                    var subjectValue = sparqlResult[sparqlResult.Variables.First()];
                    IValuedNode count = sparqlResult["count"].AsValuedNode();
                    var predicateCount = count != null ? count.AsInteger() : 0;
                    if (predicateCount <= _minValuesPerPredicateLimit)
                    {
                        continue;
                    }
                    var columnName = GetColumnName(subjectValue);
                    selectColumns.Append(columnName + " " + System.Environment.NewLine);

                    var filter = string.Format("optional{{{0} <{1}> {2}.}}{3}", subject, subjectValue, columnName, System.Environment.NewLine);
                    optionalFilters.Append(filter);
                }

                var tableQueryParameterizedString = new SparqlParameterizedString();
                tableQueryParameterizedString.Namespaces = intialQuery.NamespaceMap;
                tableQueryParameterizedString.CommandText = string.Format(@"
select distinct {0}
where
{{
{1}
{{
{2}
}}
}}
", selectColumns, optionalFilters, intialQueryCommand);

                var intialQueryCommented = "#" + intialQuery.ToString().Replace(System.Environment.NewLine, System.Environment.NewLine + "#").TrimEnd('#') + System.Environment.NewLine;
                this.OutputTableQuery = intialQueryCommented + tableQueryParameterizedString;
            }
            catch (Exception)
            {
                results = null;
                throw;
            }

            return results;
        }

        /// <summary>
        /// Get column name by replacing special chars with '_'
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private string GetColumnName(INode node)
        {
            var nodeString = node.ToString();
            if (nodeString.StartsWith("<"))
            {
                nodeString = nodeString.Substring(1, nodeString.Length - 2); //remove <>
            }

            //for a given uri: "http://www.example.org/word1/word2" and _columnNameWords = 2 the result is: word1/word2
            //for a given uri: "http://www.example.org/word1/word2" and _columnNameWords = 1 the result is: word1
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

            //replace special chars
            var validPredicate = Regex.Replace(nodeString, @"[^\d\w\s]", "_");
            return "?" + validPredicate;
        }

        /// <summary>
        /// Gets/Sets the output table query
        /// </summary>
        public string OutputTableQuery { get; private set; }
    }
}