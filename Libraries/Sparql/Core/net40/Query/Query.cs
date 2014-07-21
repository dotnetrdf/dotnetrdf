using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.Common.Collections;
using VDS.RDF.Graphs;
using VDS.RDF.Namespaces;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Elements;
using VDS.RDF.Query.Engine;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Results;
using VDS.RDF.Query.Sorting;
using VDS.RDF.Query.Templates;

namespace VDS.RDF.Query
{
    public class Query
        : IQuery
    {
        private readonly List<KeyValuePair<String, IExpression>> _projections = new List<KeyValuePair<string, IExpression>>();

        public Query()
        {
            this.Namespaces = new NamespaceMapper(true);
            this.DefaultGraphs = Enumerable.Empty<INode>();
            this.NamedGraphs = Enumerable.Empty<INode>();
        }

        public INamespaceMapper Namespaces { get; set; }

        public Uri BaseUri { get; set; }

        public IEnumerable<INode> DefaultGraphs { get; set; }

        public IEnumerable<INode> NamedGraphs { get; set; }

        public void AddDefaultGraph(INode graphName)
        {
            if (graphName == null) return;
            List<INode> defGraphs = this.DefaultGraphs != null ? this.DefaultGraphs.ToList() : new List<INode>();
            if (defGraphs.Contains(graphName)) return;
            defGraphs.Add(graphName);
            this.DefaultGraphs = defGraphs;
        }

        public void RemoveDefaultGraph(INode graphName)
        {
            if (graphName == null) return;
            List<INode> defGraphs = this.DefaultGraphs != null ? this.DefaultGraphs.ToList() : new List<INode>();
            if (defGraphs.Remove(graphName))
            {
                this.DefaultGraphs = defGraphs;
            }
        }

        public void ClearDefaultGraphs()
        {
            this.DefaultGraphs = Enumerable.Empty<INode>();
        }

        public void AddNamedGraph(INode graphName)
        {
            if (graphName == null) return;
            List<INode> namedGraphs = this.NamedGraphs != null ? this.NamedGraphs.ToList() : new List<INode>();
            if (namedGraphs.Contains(graphName)) return;
            namedGraphs.Add(graphName);
            this.NamedGraphs = namedGraphs;
        }

        public void RemoveNamedGraph(INode graphName)
        {
            if (graphName == null) return;
            List<INode> namedGraphs = this.NamedGraphs != null ? this.NamedGraphs.ToList() : new List<INode>();
            if (namedGraphs.Remove(graphName))
            {
                this.NamedGraphs = namedGraphs;
            }
        }

        public void ClearNamedGraphs()
        {
            this.NamedGraphs = Enumerable.Empty<INode>();
        }

        public QueryType QueryType { get; set; }

        public long Limit { get; set; }

        public long Offset { get; set; }

        public bool HasLimit
        {
            get { return this.Limit >= 0; }
        }

        public bool HasOffset
        {
            get { return this.Offset > 0; }
        }

        public IEnumerable<ISortCondition> SortConditions { get; set; }

        public void AddSortCondition(ISortCondition condition)
        {
            this.SortConditions = this.SortConditions == null ? condition.AsEnumerable() : this.SortConditions.Concat(condition.AsEnumerable());
        }

        public void ClearSortConditions()
        {
            this.SortConditions = Enumerable.Empty<ISortCondition>();
        }

        public IElement WhereClause { get; set; }

        public IEnumerable<IExpression> HavingConditions { get; set; }

        public IEnumerable<KeyValuePair<IExpression, string>> GroupExpressions { get; set; }

        public ITabularResults ValuesClause { get; set; }

        public ITemplate ConstructTemplate { get; set; }

        public IEnumerable<KeyValuePair<string, IExpression>> Projections
        {
            get { return new ImmutableView<KeyValuePair<String, IExpression>>(this._projections); }
            set
            {
                switch (this.QueryType)
                {
                    case QueryType.DescribeAll:
                    case QueryType.SelectAll:
                    case QueryType.SelectAllDistinct:
                    case QueryType.SelectAllReduced:
                        throw new RdfQueryException("Cannot specify a projections enumeration for a query that selects all variables");
                }

                this._projections.Clear();
                if (value == null) return;
                this._projections.AddRange(value);
                HashSet<String> seen = new HashSet<string>();
                foreach (KeyValuePair<String, IExpression> kvp in this._projections)
                {
                    if (seen.Contains(kvp.Key)) throw new RdfQueryException(String.Format("Illegal projections enumeration - variable {0} is projected multiple times", kvp.Key));
                    seen.Add(kvp.Key);
                }
            }
        }

        public void AddProjectVariable(string var)
        {
            switch (this.QueryType)
            {
                case QueryType.DescribeAll:
                case QueryType.SelectAll:
                case QueryType.SelectAllDistinct:
                case QueryType.SelectAllReduced:
                    throw new RdfQueryException("Cannot specify a projection for a query that selects all variables");
            }
            if (this._projections.Any(kvp => kvp.Key.Equals(var))) throw new RdfQueryException(String.Format("Cannot add a new projection for variable {0} as it is already projected", var));
            this._projections.Add(new KeyValuePair<string, IExpression>(var, null));
        }

        public void AddProjectExpression(string var, IExpression expr)
        {
            switch (this.QueryType)
            {
                case QueryType.DescribeAll:
                case QueryType.SelectAll:
                case QueryType.SelectAllDistinct:
                case QueryType.SelectAllReduced:
                    throw new RdfQueryException("Cannot specify a projection for a query that selects all variables");
            }
            if (this._projections.Any(kvp => kvp.Key.Equals(var))) throw new RdfQueryException(String.Format("Cannot add a new projection for variable {0} as it is already projected", var));
            this._projections.Add(new KeyValuePair<string, IExpression>(var, expr));
        }

        public void RemoveProjection(string var)
        {
            this._projections.RemoveAll(kvp => kvp.Key.Equals(var));
        }

        public void ClearProjections()
        {
            this._projections.Clear();
        }

        public IEnumerable<string> ResultVariables
        {
            get
            {
                switch (this.QueryType)
                {
                    case QueryType.SelectAll:
                    case QueryType.SelectAllDistinct:
                    case QueryType.SelectAllReduced:
                        // Result variables are all those visible in the WHERE clause
                        return this.WhereClause != null ? this.WhereClause.ProjectedVariables : Enumerable.Empty<String>();
                    case QueryType.Ask:
                    case QueryType.Construct:
                    case QueryType.Describe:
                    case QueryType.DescribeAll:
                        // No result variables for ASK/CONSTRUCT/DESCRIBE
                        return Enumerable.Empty<String>();
                    case QueryType.Select:
                    case QueryType.SelectDistinct:
                    case QueryType.SelectReduced:
                        // Only projected variables are the result variables
                        return this._projections.Select(kvp => kvp.Key);
                    default:
                        throw new RdfQueryException("Cannot determine result variables for Unknown query types, please set a QueryType first");
                }
            }
        }

        public bool Equals(IQuery other)
        {
            throw new NotImplementedException();
        }
    }
}