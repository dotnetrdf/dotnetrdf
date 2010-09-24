/*

Copyright Robert Vesse 2009-10
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
using System.Text;
using System.Text.RegularExpressions;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Tokens;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Describe;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Filters;
using VDS.RDF.Query.Grouping;
using VDS.RDF.Query.Ordering;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query
{
    /// <summary>
    /// Types of SPARQL Query
    /// </summary>
    public enum SparqlQueryType : int
    {
        /// <summary>
        /// Unknown
        /// </summary>
        Unknown= 0,
        /// <summary>
        /// Ask
        /// </summary>
        Ask = 1,
        /// <summary>
        /// Constuct
        /// </summary>
        Construct = 2,
        /// <summary>
        /// Describe
        /// </summary>
        Describe = 3,
        /// <summary>
        /// Describe All
        /// </summary>
        DescribeAll = 4,
        /// <summary>
        /// Select
        /// </summary>
        Select = 5,
        /// <summary>
        /// Select Distinct
        /// </summary>
        SelectDistinct = 6,
        /// <summary>
        /// Select Reduced
        /// </summary>
        SelectReduced = 7,
        /// <summary>
        /// Select All
        /// </summary>
        SelectAll = 8,
        /// <summary>
        /// Select All Distinct
        /// </summary>
        SelectAllDistinct = 9,
        /// <summary>
        /// Select All Reduced
        /// </summary>
        SelectAllReduced = 10
    }

    /// <summary>
    /// Types of Special SPARQL Query which may be optimised in special ways by the libraries SPARQL Engines
    /// </summary>
    public enum SparqlSpecialQueryType
    {
        /// <summary>
        /// The Query is of the form SELECT DISTINCT ?g WHERE {GRAPH ?g {?s ?p ?o}}
        /// </summary>
        DistinctGraphs,
        /// <summary>
        /// The Query has no applicable special optimisation
        /// </summary>
        NotApplicable,
        /// <summary>
        /// The Query has not yet been tested to determine if special optimisations are applicable
        /// </summary>
        Unknown,
        /// <summary>
        /// The Query is of the form ASK WHERE {?s ?p ?o}
        /// </summary>
        AskAnyTriples,
        /// <summary>
        /// The Query is of the form ASK WHERE {GRAPH ?g {?s ?p ?o}}
        /// </summary>
        AskGraphTriples
    }

    /// <summary>
    /// Available SPARQL Engines
    /// </summary>
    public enum SparqlEngine
    {
        /// <summary>
        /// Labyrinth is the original SPARQL engine which is more mature but not fully SPARQL algebra compliant
        /// </summary>
        /// <remarks>
        /// <para>
        /// Due to being obsolete now the Leviathan engine has matured significantly the Labyrinth engine was removed for the Version 0.3.0 release
        /// </para>
        /// </remarks>
        [Obsolete("Labyrinth Engine has been removed from dotNetRDF from Version 0.3.0 onwards",true)]
        Labyrinth,
        /// <summary>
        /// Leviathan is the newer and more powerful SPARQL engine which is based directly on the SPARQL algebra
        /// </summary>
        /// <remarks>
        /// <para>
        /// From Version 0.3.0 onwards Leviathan is the only SPARQL engine included in the library but the API has been enhanced so end users can introduce their own SPARQL engines or modify parts of the engines behaviour as they desire.
        /// </para>
        /// </remarks>
        Leviathan
    }

    /// <summary>
    /// Class for representing SPARQL Queries
    /// </summary>
    public sealed class SparqlQuery
    {
        private Uri _baseUri = null;
        private List<Uri> _defaultGraphs;
        private List<Uri> _namedGraphs;
        private NamespaceMapper _nsmapper;
        private SparqlQueryType _type = SparqlQueryType.Unknown;
        private SparqlSpecialQueryType _specialType = SparqlSpecialQueryType.Unknown;
        private Dictionary<String, SparqlVariable> _vars;
        private List<IToken> _describeVars = new List<IToken>();
        private GraphPattern _rootGraphPattern = null;
        private ISparqlOrderBy _orderBy = null;
        private ISparqlGroupBy _groupBy = null;
        private ISparqlFilter _having = null;
        private GraphPattern _constructTemplate;
        private BindingsPattern _bindings = null;
        private int _limit = -1;
        private int _offset = 0;
        private long _timeout = Options.QueryExecutionTimeout;
        private long _queryTime = -1;
        private long _queryTimeTicks = -1;
        private bool _partialResultsOnTimeout = false;
        private bool _optimised = false;
        private bool _subquery = false;
        private ISparqlDescribe _describer = null;

        /// <summary>
        /// Creates a new SPARQL Query
        /// </summary>
        protected internal SparqlQuery()
        {
            this._vars = new Dictionary<string, SparqlVariable>();
            this._nsmapper = new NamespaceMapper(true);
            this._defaultGraphs = new List<Uri>();
            this._namedGraphs = new List<Uri>();
        }

        /// <summary>
        /// Creates a new SPARQL Query
        /// </summary>
        /// <param name="subquery">Whether the Query is a Sub-query</param>
        protected internal SparqlQuery(bool subquery)
            : this()
        {
            this._subquery = subquery;
        }

        #region Properties

        /// <summary>
        /// Gets the Namespace Map for the Query
        /// </summary>
        public NamespaceMapper NamespaceMap
        {
            get
            {
                return this._nsmapper;
            }
            internal set
            {
                if (value != null) this._nsmapper = value;
            }
        }

        /// <summary>
        /// Gets/Sets the Base Uri for the Query
        /// </summary>
        public Uri BaseUri
        {
            get
            {
                return this._baseUri;
            }
            set
            {
                this._baseUri = value;
            }
        }

        /// <summary>
        /// Gets the Default Graph URIs for the Query
        /// </summary>
        public IEnumerable<Uri> DefaultGraphs
        {
            get
            {
                return (from u in this._defaultGraphs
                        select u);
            }
        }

        /// <summary>
        /// Gets the Named Graph URIs for the Query
        /// </summary>
        public IEnumerable<Uri> NamedGraphs
        {
            get
            {
                return (from u in this._namedGraphs
                        select u);
            }
        }

        /// <summary>
        /// Gets the Variables used in the Query
        /// </summary>
        public IEnumerable<SparqlVariable> Variables
        {
            get
            {
                return (from v in this._vars
                        select v.Value);
            }
        }

        /// <summary>
        /// Gets the Variables, QNames and URIs used in the Describe Query
        /// </summary>
        public IEnumerable<IToken> DescribeVariables
        {
            get
            {
                return (from t in this._describeVars select t);
            }
        }

        /// <summary>
        /// Gets the type of the Query
        /// </summary>
        public SparqlQueryType QueryType
        {
            get
            {
                return this._type;
            }
            internal set
            {
                this._type = value;
            }
        }

        /// <summary>
        /// Gets the Special Type of the Query (if any)
        /// </summary>
        public SparqlSpecialQueryType SpecialType
        {
            get
            {
                if (this._specialType == SparqlSpecialQueryType.Unknown)
                {
                    //Try and detect if Special Optimisations are possible
                    if (this._rootGraphPattern != null)
                    {
                        if (this._type == SparqlQueryType.Ask)
                        {
                            if (this._rootGraphPattern.ChildGraphPatterns.Count == 0 &&
                                this._rootGraphPattern.TriplePatterns.Count == 1 &&
                                this._rootGraphPattern.TriplePatterns[0].IsAcceptAll &&
                                !this._rootGraphPattern.IsFiltered)
                            {
                                this._specialType = SparqlSpecialQueryType.AskAnyTriples;
                            }
                            //REQ: Add case for AskGraphTriples
                        }
                        else if (this._type == SparqlQueryType.SelectDistinct)
                        {
                            if (this._defaultGraphs.Count == 0 &&
                                this._namedGraphs.Count == 0 &&
                                this._rootGraphPattern.TriplePatterns.Count == 0 &&
                                this._rootGraphPattern.ChildGraphPatterns.Count == 1 &&
                                this._rootGraphPattern.ChildGraphPatterns[0].TriplePatterns.Count == 1 &&
                                this._rootGraphPattern.ChildGraphPatterns[0].IsGraph &&
                                !this._rootGraphPattern.ChildGraphPatterns[0].IsFiltered &&
                                this._rootGraphPattern.ChildGraphPatterns[0].GraphSpecifier.TokenType == Token.VARIABLE &&
                                this._rootGraphPattern.ChildGraphPatterns[0].TriplePatterns[0].IsAcceptAll &&
                                this._vars[this._rootGraphPattern.ChildGraphPatterns[0].GraphSpecifier.Value.Substring(1)].IsResultVariable &&
                                this._vars.Count(pair => pair.Value.IsResultVariable) == 1)
                            {
                                this._specialType = SparqlSpecialQueryType.DistinctGraphs;
                            }
                        }
                        else
                        {
                            this._specialType = SparqlSpecialQueryType.NotApplicable;
                        }
                    }
                    else
                    {
                        this._specialType =  SparqlSpecialQueryType.NotApplicable;
                    }
                }

                return this._specialType;
            }
        }

        /// <summary>
        /// Gets the top level Graph Pattern of the Query
        /// </summary>
        public GraphPattern RootGraphPattern
        {
            get
            {
                return this._rootGraphPattern;
            }
            internal set
            {
                this._rootGraphPattern = value;
            }
        }

        /// <summary>
        /// Gets/Sets the Construct Template for a Construct Query
        /// </summary>
        public GraphPattern ConstructTemplate
        {
            get
            {
                return this._constructTemplate;
            }
            internal set
            {
                this._constructTemplate = value;
            }
        }

        /// <summary>
        /// Gets/Sets the Ordering for the Query
        /// </summary>
        public ISparqlOrderBy OrderBy
        {
            get
            {
                return this._orderBy;
            }
            internal set
            {
                this._orderBy = value;
            }
        }

        /// <summary>
        /// Gets/Sets the Grouping for the Query
        /// </summary>
        public ISparqlGroupBy GroupBy
        {
            get
            {
                return this._groupBy;
            }
            internal set
            {
                this._groupBy = value;
            }
        }

        /// <summary>
        /// Gets/Sets the Having Clause for the Query
        /// </summary>
        public ISparqlFilter Having
        {
            get
            {
                return this._having;
            }
            internal set
            {
                this._having = value;
            }
        }

        /// <summary>
        /// Gets/Sets the Bindings Clause for the Query
        /// </summary>
        public BindingsPattern Bindings
        {
            get
            {
                return this._bindings;
            }
            internal set
            {
                this._bindings = value;
            }
        }

        /// <summary>
        /// Gets/Sets the <see cref="ISparqlDescribe">ISparqlDescribe</see> which provides the Describe algorithm you wish to use
        /// </summary>
        /// <remarks>
        /// By default this will be the <see cref="ConciseBoundedDescription">ConciseBoundedDescription</see> (CBD) algorithm.
        /// </remarks>
        public ISparqlDescribe Describer
        {
            get
            {
                if (this._describer == null)
                {
                    this._describer = new ConciseBoundedDescription();
                }
                return this._describer;
            }
            set
            {
                this._describer = value;
            }
        }

        /// <summary>
        /// Gets the Result Set Limit for the Query
        /// </summary>
        /// <remarks>Values less than zero are counted as -1 which indicates no limit</remarks>
        public int Limit
        {
            get
            {
                return this._limit;
            }
            internal set
            {
                if (value > -1)
                {
                    this._limit = value;
                }
                else
                {
                    this._limit = -1;
                }
            }
        }

        /// <summary>
        /// Gets/Sets the Result Set Offset for the Query
        /// </summary>
        /// <remarks>Values less than zero are treated as 0 which indicates no offset</remarks>
        public int Offset
        {
            get
            {
                return this._offset;
            }
            internal set
            {
                if (value > 0)
                {
                    this._offset = value;
                }
                else
                {
                    this._offset = 0;
                }
            }
        }

        /// <summary>
        /// Gets/Sets the Query Execution Timeout in Milliseconds
        /// </summary>
        /// <remarks>
        /// <para>
        /// This Timeout (typically) only applies when executing the Query in memory.  If you have an instance of this class and pass its string representation (using <see cref="SparqlQuery.ToString">ToString()</see>) you will lose the timeout information as this is not serialisable in SPARQL syntax.
        /// </para>
        /// </remarks>
        public long Timeout
        {
            get
            {
                return this._timeout;
            }
            set
            {
                if (value >= 0)
                {
                    this._timeout = value;
                }
            }
        }

        /// <summary>
        /// Gets/Sets whether Partial Results should be returned in the event of Query Timeout
        /// </summary>
        /// <remarks>
        /// <para>
        /// Partial Results (typically) only applies when executing the Query in memory.  If you have an instance of this class and pass its string representation (using <see cref="SparqlQuery.ToString">ToString()</see>) you will lose the partial results information as this is not serialisable in SPARQL syntax.
        /// </para>
        /// </remarks>
        public bool PartialResultsOnTimeout
        {
            get
            {
                return this._partialResultsOnTimeout;
            }
            set
            {
                this._partialResultsOnTimeout = value;

            }
        }

        /// <summary>
        /// Gets how long the Query took in Milliseconds
        /// </summary>
        public long QueryTime
        {
            get
            {
                if (this._queryTime > -1)
                {
                    return this._queryTime;
                }
                else
                {
                    throw new RdfQueryException("Cannot request the Query Time before a Query has completed");
                }
            }
        }

        /// <summary>
        /// Gets how long the Query took in Ticks
        /// </summary>
        public long QueryTimeTicks
        {
            get
            {
                if (this._queryTimeTicks > -1)
                {
                    return this._queryTimeTicks;
                }
                else
                {
                    throw new RdfQueryException("Cannot request the Query Time before a Query has completed");
                }
            }
        }

        /// <summary>
        /// Gets whether the Query has an Aggregate as its Result
        /// </summary>
        public bool IsAggregate
        {
            get
            {
                return SparqlSpecsHelper.IsSelectQuery(this._type) && this._vars.Values.Any(v => v.IsResultVariable && v.IsAggregate);
            }
        }

        /// <summary>
        /// Gets whether Optimisation has been applied to the query
        /// </summary>
        public bool IsOptimised
        {
            get
            {
                return this._optimised;
            }
        }

        /// <summary>
        /// Checks whether a Variable is a Projection Variable
        /// </summary>
        /// <param name="var">Variable Name</param>
        /// <returns></returns>
        protected internal bool IsProjectionVariable(String var)
        {
            if (this._vars.ContainsKey(var))
            {
                return this._vars[var].IsProjection;
            }
            else
            {
                return false;
            } 
        }

        /// <summary>
        /// Gets the Projection Expression for a Projection Variable
        /// </summary>
        /// <param name="var">Variable Name</param>
        /// <returns></returns>
        protected internal ISparqlExpression  ProjectionVariable(String var)
        {
            if (this._vars.ContainsKey(var) && this._vars[var].IsProjection)
            {
                return this._vars[var].Projection;
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region Methods for setting up the Query (used by SparqlQueryParser)

        /// <summary>
        /// Adds a Variable to the Query
        /// </summary>
        /// <param name="name">Variable Name</param>
        protected internal void AddVariable(String name)
        {
            this.AddVariable(name, false);
        }

        /// <summary>
        /// Adds a Variable to the Query
        /// </summary>
        /// <param name="name">Variable Name</param>
        /// <param name="isResultVar">Does the Variable occur in the Output Result Set/Graph</param>
        protected internal void AddVariable(String name, bool isResultVar)
        {
            String var = name.Substring(1);
            if ((int)this._type >= (int)SparqlQueryType.SelectAll) isResultVar = true;

            if (!this._vars.ContainsKey(var))
            {
                this._vars.Add(var, new SparqlVariable(var, isResultVar));
            }
        }

        /// <summary>
        /// Adds a Variable to the Query
        /// </summary>
        /// <param name="var">Variable</param>
        protected internal void AddVariable(SparqlVariable var)
        {
            if (!this._vars.ContainsKey(var.Name))
            {
                this._vars.Add(var.Name, var);
            }
            else
            {
                throw new RdfQueryException("Variable ?" + var.Name + " is already defined in this Query");
            }
        }

        /// <summary>
        /// Adds a Describe Variable to the Query
        /// </summary>
        /// <param name="var">Variable/Uri/QName Token</param>
        protected internal void AddDescribeVariable(IToken var)
        {
            this._describeVars.Add(var);
        }

        /// <summary>
        /// Adds a Default Graph Uri
        /// </summary>
        /// <param name="u">Uri</param>
        public void AddDefaultGraph(Uri u)
        {
            if (!this._defaultGraphs.Contains(u))
            {
                this._defaultGraphs.Add(u);
            }
        }

        /// <summary>
        /// Adds a Named Graph Uri
        /// </summary>
        /// <param name="u"></param>
        public void AddNamedGraph(Uri u)
        {
            if (!this._namedGraphs.Contains(u))
            {
                this._namedGraphs.Add(u);
            }
        }

        #endregion

        /// <summary>
        /// Executes the SPARQL Query against the given Triple Store
        /// </summary>
        /// <param name="data">Triple Store</param>
        /// <returns>Either a <see cref="SparqlResultSet">SparqlResultSet</see> or a <see cref="Graph">Graph</see> depending on the type of query executed</returns>
        /// <remarks>
        /// Obsolete method from the removed Labyrinth engine left in the API to allow existing code to be switched over gracefully to using the Evaluate() method
        /// </remarks>
        [Obsolete("This method is an obsolete part of the now removed Labyrinth engine - use the Evaluate() method instead to invoke the Leviathan engine",true)]
        public Object Execute(IInMemoryQueryableStore data)
        {
            return this.Evaluate(data);
        }

        /// <summary>
        /// Evaluates the SPARQL Query against the given Triple Store
        /// </summary>
        /// <param name="data">Triple Store</param>
        /// <returns>
        /// Either a <see cref="SparqlResultSet">SparqlResultSet</see> or a <see cref="Graph">Graph</see> depending on the type of query executed
        /// </returns>
        /// <remarks>
        /// This method uses the more advanced and powerful Leviathan engine to evaluate queries
        /// </remarks>
        public Object Evaluate(IInMemoryQueryableStore data)
        {
            //Reset Query Timers
            this._queryTime = -1;
            this._queryTimeTicks = -1;

            bool datasetOk = false;

            try
            {
                //Set up the Default and Active Graphs
                IGraph defGraph;
                if (this._defaultGraphs.Count > 0)
                {
                    //Default Graph is the Merge of all the Graphs specified by FROM clauses
                    Graph g = new Graph();
                    foreach (Uri u in this._defaultGraphs)
                    {
                        if (data.HasGraph(u))
                        {
                            g.Merge(data.Graphs[u], true);
                        }
                        else
                        {
                            throw new RdfQueryException("A Graph with URI '" + u.ToString() + "' does not exist in this Triple Store, this URI cannot be used in a FROM clause in SPARQL queries to this Triple Store");
                        }
                    }
                    defGraph = g;
                    data.SetDefaultGraph(defGraph);
                }
                else if (this._namedGraphs.Count > 0)
                {
                    //No FROM Clauses but one/more FROM NAMED means the Default Graph is the empty graph
                    defGraph = new Graph();
                    data.SetDefaultGraph(defGraph);
                }
                else
                {
                    defGraph = null;
                    data.SetDefaultGraph(defGraph);
                }
                data.SetActiveGraph(defGraph);
                datasetOk = true;

                //Convert to Algebra and execute the Query
                SparqlEvaluationContext context = new SparqlEvaluationContext(this, data);
                BaseMultiset result;
                try
                {
                    context.StartExecution();
                    ISparqlAlgebra query = this.ToAlgebra();
                    result = query.Evaluate(context);

                    context.EndExecution();
                    this._queryTime = context.QueryTime;
                    this._queryTimeTicks = context.QueryTimeTicks;
                }
                catch (RdfQueryException)
                {
                    context.EndExecution();
                    this._queryTime = context.QueryTime;
                    this._queryTimeTicks = context.QueryTimeTicks;
                    throw;
                }
                catch
                {
                    context.EndExecution();
                    this._queryTime = context.QueryTime;
                    this._queryTimeTicks = context.QueryTimeTicks;
                    throw;
                }

                //Return the Results
                switch (this._type)
                {
                    case SparqlQueryType.Ask:
                    case SparqlQueryType.Select:
                    case SparqlQueryType.SelectAll:
                    case SparqlQueryType.SelectAllDistinct:
                    case SparqlQueryType.SelectAllReduced:
                    case SparqlQueryType.SelectDistinct:
                    case SparqlQueryType.SelectReduced:
                        //For SELECT and ASK can populate a Result Set directly from the Evaluation Context
                        return new SparqlResultSet(context);

                    case SparqlQueryType.Construct:
                        //Create a new Empty Graph for the Results
                        Graph h = new Graph();
                        h.NamespaceMap.Import(this._nsmapper);

                        //Construct the Triples for each Solution
                        foreach (Set s in context.OutputMultiset.Sets)
                        {
                            List<Triple> constructedTriples = new List<Triple>();
                            try
                            {
                                foreach (ITriplePattern p in this._constructTemplate.TriplePatterns)
                                {
                                    if (p is IConstructTriplePattern)
                                    {
                                        constructedTriples.Add(((IConstructTriplePattern)p).Construct(h, s));
                                    }
                                }
                            }
                            catch (RdfQueryException)
                            {
                                //If we throw an error this means we couldn't construct for this solution so the
                                //solution is discarded
                                continue;
                            }
                            h.Assert(constructedTriples);
                        }

                        return h;

                    case SparqlQueryType.Describe:
                    case SparqlQueryType.DescribeAll:
                        //For DESCRIBE we retrieve the Describe algorithm and apply it
                        ISparqlDescribe describer = this.Describer;
                        return describer.Describe(context);

                    default:
                        throw new NotImplementedException("Unknown query types cannot be processed by Leviathan");
                }
            }
            finally
            {
                if (datasetOk) data.ResetActiveGraph();
            }
        }

        /// <summary>
        /// Processes the Query using the given Query Processor
        /// </summary>
        /// <param name="processor">SPARQL Query Processor</param>
        /// <returns></returns>
        public Object Process(ISparqlQueryProcessor processor)
        {
            return processor.ProcessQuery(this);
        }

        /// <summary>
        /// Applies optimisation to a Query if it has not already been optimised
        /// </summary>
        public void Optimise()
        {
            if (this._optimised) return;

            if (this._rootGraphPattern != null)
            {
                this._rootGraphPattern.Optimise(Enumerable.Empty<String>());
            }

            this._optimised = true;
        }

        /// <summary>
        /// Helper method which rewrites Blank Node IDs for Describe Queries
        /// </summary>
        /// <param name="t">Triple</param>
        /// <param name="mapping">Mapping of IDs to new Blank Nodes</param>
        /// <param name="g">Graph of the Description</param>
        /// <returns></returns>
        private Triple RewriteDescribeBNodes(Triple t, Dictionary<String, INode> mapping, Graph g) {
            INode s, p, o;
            String id;

            if (t.Subject.NodeType == NodeType.Blank)
            {
                id = t.Subject.GetHashCode() + "-" + t.Graph.GetHashCode();
                if (mapping.ContainsKey(id))
                {
                    s = mapping[id];
                }
                else
                {
                    s = g.CreateBlankNode(id);
                    mapping.Add(id, s);
                }
            }
            else
            {
                s = Tools.CopyNode(t.Subject, g);
            }

            if (t.Predicate.NodeType == NodeType.Blank)
            {
                id = t.Predicate.GetHashCode() + "-" + t.Graph.GetHashCode();
                if (mapping.ContainsKey(id))
                {
                    p = mapping[id];
                }
                else
                {
                    p = g.CreateBlankNode(id);
                    mapping.Add(id, p);
                }
            }
            else
            {
                p = Tools.CopyNode(t.Predicate, g);
            }

            if (t.Object.NodeType == NodeType.Blank)
            {
                id = t.Object.GetHashCode() + "-" + t.Graph.GetHashCode();
                if (mapping.ContainsKey(id))
                {
                    o = mapping[id];
                }
                else
                {
                    o = g.CreateBlankNode(id);
                    mapping.Add(id, o);
                }
            }
            else
            {
                o = Tools.CopyNode(t.Object, g);
            }

            return new Triple(s, p, o);
        }

        /// <summary>
        /// Generates a String representation of the Query
        /// </summary>
        /// <returns></returns>
        /// <remarks>This method may not return a complete representation of the Query depending on the Query it is called on as not all the classes which can be included in a Sparql query currently implement ToString methods</remarks>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            StringBuilder from = new StringBuilder();

            //Output the Base and Prefix Directives if not a sub-query
            if (!this._subquery)
            {
                if (this._baseUri != null)
                {
                    output.AppendLine("BASE <" + this._baseUri.ToString() + ">");
                }
                foreach (String prefix in this._nsmapper.Prefixes)
                {
                    output.AppendLine("PREFIX " + prefix + ": <" + this._nsmapper.GetNamespaceUri(prefix).ToString() + ">");
                }
                if (output.Length > 0)
                {
                    output.AppendLine();
                }

                //Build the String for the FROM clause
                if (this._defaultGraphs.Count > 0 || this._namedGraphs.Count > 0) from.Append(' ');
                foreach (Uri u in this._defaultGraphs)
                {
                    from.AppendLine("FROM <" + u.ToString() + ">");
                }
                foreach (Uri u in this._namedGraphs)
                {
                    from.AppendLine("FROM NAMED <" + u.ToString() + ">");
                }
            }

            switch (this._type)
            {
                case SparqlQueryType.Ask:
                    output.Append("ASK");
                    if (from.Length > 0)
                    {
                        output.Append(from.ToString());
                    }
                    else
                    {
                        output.Append(' ');
                    }
                    output.Append("WHERE ");
                    break;

                case SparqlQueryType.Construct:
                    output.Append("CONSTRUCT ");
                    output.Append(this._constructTemplate.ToString());
                    if (this._constructTemplate.TriplePatterns.Count > 1)
                    {
                        output.AppendLine();
                    }
                    else
                    {
                        output.Append(' ');
                    }
                    output.Append(from.ToString());
                    output.AppendLine("WHERE ");
                    break;

                case SparqlQueryType.Describe:
                    output.Append("DESCRIBE ");
                    foreach (IToken dvar in this._describeVars)
                    {
                        switch (dvar.TokenType)
                        {
                            case Token.URI:
                                output.Append("<" + dvar.Value + "> ");
                                break;
                            case Token.QNAME:
                            case Token.VARIABLE:
                            default:
                                output.Append(dvar.Value + " ");
                                break;
                        }
                    }
                    output.Append(from.ToString());
                    if (this._rootGraphPattern != null)
                    {
                        output.AppendLine("WHERE");
                    }
                    break;

                case SparqlQueryType.DescribeAll:
                    output.Append("DESCRIBE * ");
                    output.Append(from.ToString());
                    if (this._rootGraphPattern != null)
                    {
                        output.Append("WHERE");
                    }
                    break;

                case SparqlQueryType.Select:
                case SparqlQueryType.SelectAll:
                case SparqlQueryType.SelectAllDistinct:
                case SparqlQueryType.SelectAllReduced:
                case SparqlQueryType.SelectDistinct:
                case SparqlQueryType.SelectReduced:
                    output.Append("SELECT ");
                    if (this._type == SparqlQueryType.SelectAllDistinct || this._type == SparqlQueryType.SelectDistinct)
                    {
                        output.Append("DISTINCT ");
                    }
                    else if (this._type == SparqlQueryType.SelectAllReduced || this._type == SparqlQueryType.SelectReduced)
                    {
                        output.Append("REDUCED ");
                    }
                    if ((int)this._type >= (int)SparqlQueryType.SelectAll)
                    {
                        output.Append('*');
                        if (from.Length > 0) {
                            output.Append(from.ToString());
                        } else {
                            output.Append(' ');
                        }
                        output.AppendLine("WHERE ");
                    }
                    else
                    {
                        foreach (SparqlVariable var in this._vars.Values)
                        {
                            if (var.IsResultVariable)
                            {
                                output.Append(var.ToString() + " ");
                            }
                        }
                        if (from.Length > 0) output.Append(from.ToString().Substring(1));
                        output.AppendLine("WHERE");                       
                    }
                    break;
            }

            if (this._rootGraphPattern != null)
            {
                if (this._rootGraphPattern.IsEmpty && (int)this._type >= (int)SparqlQueryType.Select)
                {
                    output.Remove(output.Length - 2, 2);
                    output.Append(" ");
                    output.Append(this._rootGraphPattern.ToString());
                }
                else
                {
                    output.AppendLine(this._rootGraphPattern.ToString());
                }
            }

            if (this._groupBy != null)
            {
                output.Append("GROUP BY ");
                output.Append(this._groupBy.ToString());
                output.Append(' ');
            }
            if (this._having != null)
            {
                output.Append("HAVING ");
                String having = this._having.ToString();
                output.Append(having.Substring(7, having.Length - 8));
                output.Append(' ');
            }

            if (this._orderBy != null)
            {
                output.Append("ORDER BY ");
                output.Append(this._orderBy.ToString());
            }

            if (this._limit > -1)
            {
                output.Append("LIMIT " + this._limit + " ");
            }
            if (this._offset > 0)
            {
                output.Append("OFFSET " + this._offset);
            }
            if (this._bindings != null)
            {
                output.AppendLine();
                output.AppendLine(this._bindings.ToString());
            }

            String preOutput = output.ToString();
            preOutput = preOutput.Replace("<" + RdfSpecsHelper.RdfType + ">", "a");
            if (this._nsmapper.Prefixes.Any())
            {
                foreach (String prefix in this._nsmapper.Prefixes)
                {
                    String uri = this._nsmapper.GetNamespaceUri(prefix).ToString();
                    if (preOutput.Contains("<" + uri))
                    {
                        preOutput = Regex.Replace(preOutput, "<" + uri + "([^/#>]+)>\\.", prefix + ":$1 .");
                        preOutput = Regex.Replace(preOutput, "<" + uri + "([^/#>]+)>", prefix + ":$1");
                    }
                }
            }

            return preOutput;
        }

        /// <summary>
        /// Converts the Query into it's SPARQL Algebra representation (as represented in the Leviathan API)
        /// </summary>
        /// <returns></returns>
        public ISparqlAlgebra ToAlgebra()
        {
            ISparqlAlgebra pattern;
            if (this._rootGraphPattern != null)
            {
                switch (this.SpecialType)
                {
                    case SparqlSpecialQueryType.DistinctGraphs:
                        pattern = new SelectDistinctGraphs();
                        break;
                    case SparqlSpecialQueryType.AskAnyTriples:
                        pattern = new AskAnyTriples();
                        break;
                    case SparqlSpecialQueryType.NotApplicable:
                    default:
                        pattern = this._rootGraphPattern.ToAlgebra();
                        break;
                }
            }
            else
            {
                pattern = new BGP();
            }

            //If we have a BINDINGS clause then we'll Join in into the algebra here
            if (this._bindings != null)
            {
                pattern = new Bindings(this._bindings, pattern);
            }

            //Q: Wrap the entire thing in an Algebra that initialises the Dataset?

            switch (this._type)
            {
                case SparqlQueryType.Ask:
                    return new Ask(pattern);

                case SparqlQueryType.Construct:
                case SparqlQueryType.Describe:
                case SparqlQueryType.DescribeAll:
                case SparqlQueryType.Select:
                case SparqlQueryType.SelectAll:
                case SparqlQueryType.SelectAllDistinct:
                case SparqlQueryType.SelectAllReduced:
                case SparqlQueryType.SelectDistinct:
                case SparqlQueryType.SelectReduced:
                    //GROUP BY is the first thing applied
                    if (this._groupBy != null) pattern = new GroupBy(pattern);

                    //After grouping we do projection
                    //This will generate the values for any Project Expressions and Aggregates
                    pattern = new Project(pattern);

                    //Add HAVING clause after the projection
                    if (this._having != null) pattern = new Having(pattern);

                    //We can then Order our results
                    //We do ordering before we do Select but after Project so we can order by any of
                    //the project expressions/aggregates and any variable in the results even if
                    //it won't be output as a result variable
                    if (this._orderBy != null) pattern = new OrderBy(pattern);

                    //After Ordering we apply Select
                    //Select effectively trims the results so only result variables are left
                    //This doesn't apply to CONSTRUCT since any variable may be used in the Construct Template
                    //so we don't want to eliminate anything
                    if (this._type != SparqlQueryType.Construct) pattern = new Select(pattern);

                    //If we have a Distinct/Reduced then we'll apply those after Selection
                    if (this._type == SparqlQueryType.SelectAllDistinct || this._type == SparqlQueryType.SelectDistinct)
                    {
                        pattern = new Distinct(pattern);
                    }
                    else if (this._type == SparqlQueryType.SelectAllReduced || this._type == SparqlQueryType.SelectReduced)
                    {
                        pattern = new Reduced(pattern);
                    }

                    //Finally we can apply any limit and/or offset
                    if (this._limit >= 0 || this._offset > 0)
                    {
                        pattern = new Slice(pattern);
                    }

                    return pattern;

                default:
                    throw new RdfQueryException("Unable to convert unknown Query Types to SPARQL Algebra");
            }
        }
    }
}
