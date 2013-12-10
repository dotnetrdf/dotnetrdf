using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using VDS.RDF.Query;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager.Tasks
{
    public class QueryAsTableTask : QueryTask
    {
        private int _minValuesPerPredicateLimit;

        public QueryAsTableTask(IQueryableStorage manager, string query, int minValuesPerPredicateLimit)
            : base(manager, query)
        {
            _minValuesPerPredicateLimit = minValuesPerPredicateLimit;
        }

        public QueryAsTableTask(IQueryableStorage manager, string query, int pageSize, int minValuesPerPredicateLimit)
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

            SparqlVariable subject;

            SparqlQuery intialQuery = this._parser.ParseFromString(this._query);

            if (intialQuery.QueryType != SparqlQueryType.Select && intialQuery.QueryType != SparqlQueryType.SelectDistinct)
            {
                throw new RdfQueryException("Only Sparql select Query with variables is supported for table format");
            }

            //separate prefixes from query command (since it's a select q type search for "select" keyword)
            var indexOfSelect = this._query.ToLower().IndexOf("select", System.StringComparison.Ordinal);
            var intialQueryPrefixes = this._query.Substring(0, indexOfSelect);
            var intialQueryCommand = this._query.Remove(0, indexOfSelect);
            subject = intialQuery.Variables.First();
           
            var getPredicatesQuery = string.Format(@"{0}
select distinct ?p (COUNT(?p) as ?count)
where
{{
{1} ?p ?o.
{{
{2}
}}
}}
GROUP BY ?p
", intialQueryPrefixes, subject, intialQueryCommand);


            this._query = getPredicatesQuery;

            //get predicates and nr of usages
            var results = base.RunTaskInternal();

            //contruct second query for final result
            try
            {
                var sparqlResults =  results as SparqlResultSet;

                if (sparqlResults == null)
                {
                    throw new RdfQueryException("Only SparqlQuery is supported for table format");
                }

                var selectColumns = new StringBuilder();
                selectColumns.Append(subject + " ");

                var optionlaFilters = new StringBuilder();
            
                //foreach predicate add a column and an "optional" query clause
                foreach (var sparqlResult in sparqlResults)
                {
                    var subjectValue = sparqlResult[sparqlResult.Variables.First()];
                    var count = sparqlResult["count"] as LiteralNode;
                    var predicateCount = Int32.Parse(count.Value);
                    if (predicateCount <= _minValuesPerPredicateLimit)
                    {
                        continue;
                    }
                    var columnName = GetColumnName(subjectValue);
                    selectColumns.Append(columnName + " ");

                    var filter = string.Format("optional{{{0} <{1}> {2}.}}", subject, subjectValue, columnName);
                    optionlaFilters.Append(filter);
                }

                var tableQuery = string.Format(@"{0}
select distinct {1}
where
{{
{2}
{{
{3}
}}
}}
", intialQueryPrefixes, selectColumns, optionlaFilters, intialQueryCommand);

                this._query = tableQuery;

                //execute final query
                results = base.RunTaskInternal();
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
            var isUri = false;
            if (nodeString.StartsWith("<"))
            {
                isUri = true;
                nodeString = nodeString.Substring(1, nodeString.Length - 1);
            }
            //replace special chars
            var validPredicate = Regex.Replace(nodeString, @"[^\d\w\s]", "_");
            return "?"+validPredicate;
        }



    }
}
