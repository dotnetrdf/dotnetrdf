/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2020 dotNetRDF Project (http://dotnetrdf.org/)
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
using System.Text;
using System.Text.RegularExpressions;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Tokens;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Builder;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Query.Describe;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Filters;
using VDS.RDF.Query.Grouping;
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Query.Ordering;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Query.PropertyFunctions;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query
{
    /// <summary>
    /// Represents a SPARQL Query.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Note:</strong> This class is purposefully sealed and most setters are private/protected internal since generally you create a query by using the <see cref="SparqlQueryParser"/> to parse a query string/file.
    /// </para>
    /// <para>
    /// To build a query programmatically you can use the <see cref="QueryBuilder"/> class to generate a new query and then various extension methods to modify that query using a fluent style API.  A query is not immutable
    /// so if you use that API you are modifying the query, if you want to generate new queries by modifying an existing query consider using the <see cref="SparqlQuery.Copy()"/> method to take a copy of the existing query.
    /// </para>
    /// </remarks>
    public sealed class SparqlQuery
        : NodeFactory
    {
        private List<IRefNode> _defaultGraphs;
        private List<IRefNode> _namedGraphs;
        private SparqlQueryType _type = SparqlQueryType.Unknown;
        private SparqlSpecialQueryType _specialType = SparqlSpecialQueryType.Unknown;
        private List<SparqlVariable> _vars;
        private List<IToken> _describeVars = new List<IToken>();
        private GraphPattern _rootGraphPattern = null;
        private ISparqlOrderBy _orderBy = null;
        private ISparqlGroupBy _groupBy = null;
        private ISparqlFilter _having = null;
        private GraphPattern _constructTemplate;
        private BindingsPattern _bindings = null;
        private int _limit = -1;
        private int _offset = 0;
        private long _timeout = 0;
        private TimeSpan? _executionTime = null;
        private bool _partialResultsOnTimeout = false;
        private bool _optimised = false;
        private bool? _optimisableOrdering;
        private bool _subquery = false;
        private ISparqlDescribe _describer = null;
        private IEnumerable<IAlgebraOptimiser> _optimisers = Enumerable.Empty<IAlgebraOptimiser>();
        private IEnumerable<ISparqlCustomExpressionFactory> _exprFactories = Enumerable.Empty<ISparqlCustomExpressionFactory>();
        private IEnumerable<IPropertyFunctionFactory> _propFuncFactories = Enumerable.Empty<IPropertyFunctionFactory>();

        /// <summary>
        /// Creates a new SPARQL Query.
        /// </summary>
        internal SparqlQuery(Uri baseUri = null, INamespaceMapper namespaceMapper = null, bool subquery = false) : 
            base(baseUri, namespaceMapper ?? new NamespaceMapper(true))
        {
            _vars = new List<SparqlVariable>();
            _defaultGraphs = new List<IRefNode>();
            _namedGraphs = new List<IRefNode>();
            _subquery = subquery;
        }

        /// <summary>
        /// Creates a new SPARQL Query.
        /// </summary>
        /// <param name="subquery">Whether the Query is a Sub-query.</param>
        internal SparqlQuery(bool subquery): this(null, null, subquery)
        {
        }

        /// <summary>
        /// Creates a copy of the query.
        /// </summary>
        /// <returns></returns>
        public SparqlQuery Copy()
        {
            var q = new SparqlQuery(BaseUri, NamespaceMap, _subquery);
            q._defaultGraphs = new List<IRefNode>(_defaultGraphs);
            q._namedGraphs = new List<IRefNode>(_namedGraphs);
            q._type = _type;
            q._specialType = _specialType;
            q._vars = new List<SparqlVariable>(_vars);
            q._describeVars = new List<IToken>(_describeVars);
            if(_rootGraphPattern != null)
                q._rootGraphPattern = new GraphPattern(_rootGraphPattern);
            q._orderBy = _orderBy;
            q._groupBy = _groupBy;
            q._having = _having;
            q._constructTemplate = _constructTemplate;
            q._bindings = _bindings;
            q._limit = _limit;
            q._offset = _offset;
            q._timeout = _timeout;
            q._partialResultsOnTimeout = _partialResultsOnTimeout;
            q._optimised = _optimised;
            q._describer = _describer;
            q._optimisers = new List<IAlgebraOptimiser>(_optimisers);
            q._exprFactories = new List<ISparqlCustomExpressionFactory>(_exprFactories);
            q._propFuncFactories = new List<IPropertyFunctionFactory>(_propFuncFactories);
            return q;
        }

        #region Properties

        /// <summary>
        /// Gets the Default Graph URIs for the Query.
        /// </summary>
        [Obsolete("Replaced by DefaultGraphNames")]
        public IEnumerable<Uri> DefaultGraphs => _defaultGraphs.Where(x=>x is null || x.NodeType == NodeType.Uri).Select(x=>(x as IUriNode)?.Uri);

        /// <summary>
        /// Gets the names of the default graphs for the query.
        /// </summary>
        public IEnumerable<IRefNode> DefaultGraphNames => _defaultGraphs.AsReadOnly();

        /// <summary>
        /// Gets the Named Graph URIs for the Query.
        /// </summary>
        [Obsolete("Replaced by NamedGraphNames")]
        public IEnumerable<Uri> NamedGraphs => _namedGraphs.Where(x => x is null || x.NodeType == NodeType.Uri)
            .Select(x => (x as IUriNode)?.Uri);

        /// <summary>
        /// Gets the names of the named graphs for the query.
        /// </summary>
        public IEnumerable<IRefNode> NamedGraphNames => _namedGraphs.AsReadOnly();

        /// <summary>
        /// Gets the Variables used in the Query.
        /// </summary>
        public IEnumerable<SparqlVariable> Variables => _vars;

        /// <summary>
        /// Gets the Variables, QNames and URIs used in the Describe Query.
        /// </summary>
        public IEnumerable<IToken> DescribeVariables => (from t in _describeVars select t);

        /// <summary>
        /// Gets the type of the Query.
        /// </summary>
        public SparqlQueryType QueryType
        {
            get => _type;
            internal set => _type = value;
        }

        /// <summary>
        /// Gets the Special Type of the Query (if any).
        /// </summary>
        public SparqlSpecialQueryType SpecialType
        {
            get
            {
                if (_specialType == SparqlSpecialQueryType.Unknown)
                {
                    // Try and detect if Special Optimisations are possible
                    if (_rootGraphPattern != null)
                    {
                        if (_type == SparqlQueryType.Ask)
                        {
                            if (_rootGraphPattern.ChildGraphPatterns.Count == 0 &&
                                _rootGraphPattern.TriplePatterns.Count == 1 &&
                                _rootGraphPattern.TriplePatterns[0].IsAcceptAll &&
                                !_rootGraphPattern.IsFiltered)
                            {
                                _specialType = SparqlSpecialQueryType.AskAnyTriples;
                            }
                        }
                        else if (_type == SparqlQueryType.SelectDistinct)
                        {
                            if (_defaultGraphs.Count == 0 &&
                                _namedGraphs.Count == 0 &&
                                _rootGraphPattern.TriplePatterns.Count == 0 &&
                                _rootGraphPattern.ChildGraphPatterns.Count == 1 &&
                                _rootGraphPattern.ChildGraphPatterns[0].TriplePatterns.Count == 1 &&
                                _rootGraphPattern.ChildGraphPatterns[0].IsGraph &&
                                !_rootGraphPattern.ChildGraphPatterns[0].IsFiltered &&
                                _rootGraphPattern.ChildGraphPatterns[0].GraphSpecifier.TokenType == Token.VARIABLE &&
                                _rootGraphPattern.ChildGraphPatterns[0].TriplePatterns[0].IsAcceptAll &&
                                _vars[0].IsResultVariable && 
                                _rootGraphPattern.ChildGraphPatterns[0].GraphSpecifier.Value.Substring(1).Equals(_vars[0].Name) &&
                                _vars.Count(v => v.IsResultVariable) == 1)
                            {
                                _specialType = SparqlSpecialQueryType.DistinctGraphs;
                            }
                        }
                        else
                        {
                            _specialType = SparqlSpecialQueryType.NotApplicable;
                        }
                    }
                    else
                    {
                        _specialType =  SparqlSpecialQueryType.NotApplicable;
                    }
                }

                return _specialType;
            }
        }

        /// <summary>
        /// Gets the top level Graph Pattern of the Query.
        /// </summary>
        public GraphPattern RootGraphPattern
        {
            get => _rootGraphPattern;
            internal set => _rootGraphPattern = value;
        }

        /// <summary>
        /// Gets/Sets the Construct Template for a Construct Query.
        /// </summary>
        public GraphPattern ConstructTemplate
        {
            get => _constructTemplate;
            internal set => _constructTemplate = value;
        }

        /// <summary>
        /// Gets/Sets the Ordering for the Query.
        /// </summary>
        public ISparqlOrderBy OrderBy
        {
            get => _orderBy;
            internal set
            {
                _orderBy = value;
                _optimisableOrdering = null; //When ORDER BY gets set reset whether the Ordering is optimisable
            }
        }

        /// <summary>
        /// Gets/Sets the Grouping for the Query.
        /// </summary>
        public ISparqlGroupBy GroupBy
        {
            get => _groupBy;
            internal set => _groupBy = value;
        }

        /// <summary>
        /// Gets/Sets the Having Clause for the Query.
        /// </summary>
        public ISparqlFilter Having
        {
            get => _having;
            internal set => _having = value;
        }

        /// <summary>
        /// Gets/Sets the VALUES Clause for the Query which are bindings that should be applied.
        /// </summary>
        public BindingsPattern Bindings
        {
            get => _bindings;
            internal set => _bindings = value;
        }

        /// <summary>
        /// Gets/Sets the <see cref="ISparqlDescribe">ISparqlDescribe</see> which provides the Describe algorithm you wish to use.
        /// </summary>
        /// <remarks>
        /// By default this will be the <see cref="ConciseBoundedDescription">ConciseBoundedDescription</see> (CBD) algorithm.
        /// </remarks>
        public ISparqlDescribe Describer
        {
            get
            {
                if (_describer == null)
                {
                    _describer = new ConciseBoundedDescription();
                }
                return _describer;
            }
            set => _describer = value;
        }

        /// <summary>
        /// Gets/Sets the locally scoped Algebra Optimisers that are used to optimise the Query Algebra in addition to (but before) any global optimisers (specified by <see cref="SparqlOptimiser.AlgebraOptimisers">SparqlOptimiser.AlgebraOptimisers</see>) that are applied.
        /// </summary>
        public IEnumerable<IAlgebraOptimiser> AlgebraOptimisers
        {
            get => _optimisers;
            set
            {
                if (value != null)
                {
                    _optimisers = value;
                }
                else
                {
                    _optimisers = Enumerable.Empty<IAlgebraOptimiser>();
                }
            }
        }

        /// <summary>
        /// Gets/Sets the locally scoped Expression Factories that may be used if the query is using the CALL() function to do dynamic function invocation.
        /// </summary>
        public IEnumerable<ISparqlCustomExpressionFactory> ExpressionFactories
        {
            get => _exprFactories;
            set
            {
                if (value != null)
                {
                    _exprFactories = value;
                }
                else
                {
                    _exprFactories = Enumerable.Empty<ISparqlCustomExpressionFactory>();
                }
            }
        }

        /// <summary>
        /// Gets/Sets the locally scoped Property Function factories that may be used by the <see cref="PropertyFunctionOptimiser"/> when generating the algebra for the query.
        /// </summary>
        public IEnumerable<IPropertyFunctionFactory> PropertyFunctionFactories
        {
            get => _propFuncFactories;
            set
            {
                if (value != null)
                {
                    _propFuncFactories = value;
                }
                else
                {
                    _propFuncFactories = Enumerable.Empty<IPropertyFunctionFactory>();
                }
            }
        }

        /// <summary>
        /// Gets the Result Set Limit for the Query.
        /// </summary>
        /// <remarks>Values less than zero are counted as -1 which indicates no limit.</remarks>
        public int Limit
        {
            get => _limit;
            set
            {
                if (value > -1)
                {
                    _limit = value;
                }
                else
                {
                    _limit = -1;
                }
            }
        }

        /// <summary>
        /// Gets/Sets the Result Set Offset for the Query.
        /// </summary>
        /// <remarks>Values less than zero are treated as 0 which indicates no offset.</remarks>
        public int Offset
        {
            get => _offset;
            set
            {
                if (value > 0)
                {
                    _offset = value;
                }
                else
                {
                    _offset = 0;
                }
            }
        }

        /// <summary>
        /// Gets/Sets the Query Execution Timeout in milliseconds.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This Timeout (typically) only applies when executing the Query in memory.  If you have an instance of this class and pass its string representation (using <see cref="SparqlQuery.ToString">ToString()</see>) you will lose the timeout information as this is not serialisable in SPARQL syntax.
        /// </para>
        /// </remarks>
        public long Timeout
        {
            get => _timeout;
            set => _timeout = Math.Max(value, 0);
        }

        /// <summary>
        /// Gets/Sets whether Partial Results should be returned in the event of Query Timeout.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Partial Results (typically) only applies when executing the Query in memory.  If you have an instance of this class and pass its string representation (using <see cref="SparqlQuery.ToString">ToString()</see>) you will lose the partial results information as this is not serialisable in SPARQL syntax.
        /// </para>
        /// </remarks>
        public bool PartialResultsOnTimeout
        {
            get => _partialResultsOnTimeout;
            set => _partialResultsOnTimeout = value;
        }

        /// <summary>
        /// Gets the Time taken to execute a Query.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if you try and inspect the execution time before the Query has been executed.</exception>
        public TimeSpan? QueryExecutionTime
        {
            get
            {
                if (!_executionTime.HasValue)
                {
                    throw new InvalidOperationException("Cannot inspect the Query Time as the Query has not yet been processed");
                }
                return _executionTime;
            }
            set => _executionTime = value;
        }

        /// <summary>
        /// Gets whether the Query has an Aggregate as its Result.
        /// </summary>
        public bool IsAggregate
        {
            get
            {
                return SparqlSpecsHelper.IsSelectQuery(_type) && _vars.Any(v => v.IsResultVariable && v.IsAggregate);
            }
        }

        /// <summary>
        /// Gets whether Optimisation has been applied to the query.
        /// </summary>
        /// <remarks>
        /// This only indicates that an Optimiser has been applied.  You can always reoptimise the query using a different optimiser by using the relevant overload of the <see cref="SparqlQuery.Optimise()">Optimise()</see> method.
        /// </remarks>
        public bool IsOptimised => _optimised;

        /// <summary>
        /// Gets whether this Query is a Sub-Query in another Query.
        /// </summary>
        public bool IsSubQuery => _subquery;

        /// <summary>
        /// Gets whether a Query has a DISTINCT modifier.
        /// </summary>
        public bool HasDistinctModifier
        {
            get
            {
                switch (_type)
                {
                    case SparqlQueryType.SelectAllDistinct:
                    case SparqlQueryType.SelectDistinct:
                        return true;
                    default:
                        return false;
                }
            }
        }

        /// <summary>
        /// Gets whether the Query has a Solution Modifier (a GROUP BY, HAVING, ORDER BY, LIMIT or OFFSET).
        /// </summary>
        public bool HasSolutionModifier => _groupBy != null || _having != null || _orderBy != null || _limit >= 0 || _offset > 0;

        /// <summary>
        /// The number of results that would be returned without any limit clause to a query or -1 if not supported. Defaults to the same value as the Count member.
        /// </summary>
        public int VirtualCount { get; internal set; } = -1;

        #endregion

        #region Methods for setting up the Query (used by SparqlQueryParser)

        /// <summary>
        /// Adds a Variable to the Query.
        /// </summary>
        /// <param name="name">Variable Name.</param>
        internal void AddVariable(string name)
        {
            AddVariable(name, false);
        }

        /// <summary>
        /// Adds a Variable to the Query.
        /// </summary>
        /// <param name="name">Variable Name.</param>
        /// <param name="isResultVar">Does the Variable occur in the Output Result Set/Graph.</param>
        internal void AddVariable(string name, bool isResultVar)
        {
            var var = name.Substring(1);
            if ((int)_type >= (int)SparqlQueryType.SelectAll) isResultVar = true;

            if (!_vars.Any(v => v.Name.Equals(var)))
            {
                _vars.Add(new SparqlVariable(var, isResultVar));
            }
        }

        /// <summary>
        /// Adds a Variable to the Query.
        /// </summary>
        /// <param name="var">Variable.</param>
        internal void AddVariable(SparqlVariable var)
        {
            if (!_vars.Any(v => v.Name.Equals(var.Name)))
            {
                _vars.Add(var);
            }
            else
            {
                throw new RdfQueryException("Variable ?" + var.Name + " is already defined in this Query");
            }
        }

        /// <summary>
        /// Adds a Describe Variable to the Query.
        /// </summary>
        /// <param name="var">Variable/Uri/QName Token.</param>
        internal void AddDescribeVariable(IToken var)
        {
            _describeVars.Add(var);
        }

        /// <summary>
        /// Adds a Default Graph URI.
        /// </summary>
        /// <param name="u">Graph URI.</param>
        [Obsolete("Replaced by AddDefaultGraph(IRefNode)")]
        public void AddDefaultGraph(Uri u)
        {
            AddDefaultGraph(new UriNode(u));
        }

        /// <summary>
        /// Adds a Named Graph URI.
        /// </summary>
        /// <param name="u">Graph URI.</param>
        [Obsolete("Replaced by AddNamedGraph(IRefNode)")]
        public void AddNamedGraph(Uri u)
        {
            AddNamedGraph(new UriNode(u));
        }

        /// <summary>
        /// Adds a graph to the default graph of the query.
        /// </summary>
        /// <param name="n"></param>
        public void AddDefaultGraph(IRefNode n)
        {
            if (!_defaultGraphs.Contains(n)) _defaultGraphs.Add(n);
        }
        
        /// <summary>
        /// Adds a named graph to the query.
        /// </summary>
        /// <param name="n"></param>
        public void AddNamedGraph(IRefNode n)
        {
            if (!_namedGraphs.Contains(n)) _namedGraphs.Add(n);
        }
        /// <summary>
        /// Removes all Default Graph URIs.
        /// </summary>
        public void ClearDefaultGraphs()
        {
            _defaultGraphs.Clear();
        }

        /// <summary>
        /// Removes all Named Graph URIs.
        /// </summary>
        public void ClearNamedGraphs()
        {
            _namedGraphs.Clear();
        }

        #endregion


        /// <summary>
        /// Processes the Query using the given Query Processor.
        /// </summary>
        /// <param name="processor">SPARQL Query Processor.</param>
        /// <returns></returns>
        public object Process(ISparqlQueryProcessor processor)
        {
            return processor.ProcessQuery(this);
        }

        /// <summary>
        /// Applies optimisation to a Query using the default global optimiser.
        /// </summary>
        public void Optimise()
        {
            Optimise(SparqlOptimiser.QueryOptimiser);
        }

        /// <summary>
        /// Applies optimisation to a Query using the specific optimiser.
        /// </summary>
        /// <param name="optimiser">Query Optimiser.</param>
        public void Optimise(IQueryOptimiser optimiser)
        {
            if (optimiser == null) throw new ArgumentNullException(nameof(optimiser), "Cannot optimise a Query using a null optimiser");
            if (_rootGraphPattern != null)
            {
                optimiser.Optimise(_rootGraphPattern, Enumerable.Empty<string>());
            }

            _optimised = true;
        }


        /// <summary>
        /// Generates a String representation of the Query.
        /// </summary>
        /// <returns></returns>
        /// <remarks>This method may not return a complete representation of the Query depending on the Query it is called on as not all the classes which can be included in a Sparql query currently implement ToString methods.</remarks>
        public override string ToString()
        {
            var output = new StringBuilder();
            var from = new StringBuilder();
            var formatter = new SparqlFormatter();

            // Output the Base and Prefix Directives if not a sub-query
            if (!_subquery)
            {
                if (BaseUri != null)
                {
                    output.AppendLine("BASE <" + BaseUri.AbsoluteUri + ">");
                }
                foreach (var prefix in NamespaceMap.Prefixes)
                {
                    output.AppendLine("PREFIX " + prefix + ": <" + NamespaceMap.GetNamespaceUri(prefix).AbsoluteUri + ">");
                }
                if (output.Length > 0)
                {
                    output.AppendLine();
                }

                // Build the String for the FROM clause
                if (_defaultGraphs.Count > 0 || _namedGraphs.Count > 0) from.Append(' ');
                foreach (IRefNode n in _defaultGraphs.Where(n => n != null))
                {
                    from.AppendLine("FROM " + formatter.Format(n));
                }
                foreach (IRefNode n in _namedGraphs.Where(n => n != null))
                {
                    from.AppendLine("FROM NAMED " + formatter.Format(n));
                }
            }

            switch (_type)
            {
                case SparqlQueryType.Ask:
                    output.Append("ASK");
                    if (from.Length > 0)
                    {
                        output.Append(@from);
                    }
                    else
                    {
                        output.Append(' ');
                    }
                    output.Append("WHERE ");
                    break;

                case SparqlQueryType.Construct:
                    output.Append("CONSTRUCT ");
                    output.Append(_constructTemplate);
                    if (_constructTemplate.TriplePatterns.Count > 1)
                    {
                        output.AppendLine();
                    }
                    else
                    {
                        output.Append(' ');
                    }
                    output.Append(@from);
                    output.AppendLine("WHERE ");
                    break;

                case SparqlQueryType.Describe:
                    output.Append("DESCRIBE ");
                    foreach (IToken dvar in _describeVars)
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
                    output.Append(@from);
                    if (_rootGraphPattern != null)
                    {
                        output.AppendLine("WHERE");
                    }
                    break;

                case SparqlQueryType.DescribeAll:
                    output.Append("DESCRIBE * ");
                    output.Append(@from);
                    if (_rootGraphPattern != null)
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
                    if (_type == SparqlQueryType.SelectAllDistinct || _type == SparqlQueryType.SelectDistinct)
                    {
                        output.Append("DISTINCT ");
                    }
                    else if (_type == SparqlQueryType.SelectAllReduced || _type == SparqlQueryType.SelectReduced)
                    {
                        output.Append("REDUCED ");
                    }
                    if ((int)_type >= (int)SparqlQueryType.SelectAll)
                    {
                        output.Append('*');
                        if (from.Length > 0) 
                        {
                            output.Append(@from);
                        } 
                        else 
                        {
                            output.Append(' ');
                        }
                        output.AppendLine("WHERE ");
                    }
                    else
                    {
                        foreach (SparqlVariable var in _vars)
                        {
                            if (var.IsResultVariable)
                            {
                                output.Append(var);
                                output.Append(' ');
                            }
                        }
                        if (from.Length > 0) output.Append(from.ToString().Substring(1));
                        output.AppendLine("WHERE");                       
                    }
                    break;
            }

            if (_rootGraphPattern != null)
            {
                if (_rootGraphPattern.IsEmpty && (int)_type >= (int)SparqlQueryType.Select)
                {
                    output.Remove(output.Length - 2, 2);
                    output.Append(" ");
                    output.Append(_rootGraphPattern);
                }
                else if (_rootGraphPattern.HasModifier)
                {
                    output.AppendLine("{");
                    output.AppendLine(_rootGraphPattern.ToString());
                    output.AppendLine("}");
                }
                else
                {
                    output.AppendLine(_rootGraphPattern.ToString());
                }
            }

            if (_groupBy != null)
            {
                output.Append("GROUP BY ");
                output.Append(_groupBy);
                output.Append(' ');
            }
            if (_having != null)
            {
                output.Append("HAVING ");
                var having = _having.ToString();
                output.Append(having.Substring(7, having.Length - 8));
                output.Append(' ');
            }

            if (_orderBy != null)
            {
                output.Append("ORDER BY ");
                output.Append(_orderBy);
            }

            if (_limit > -1)
            {
                output.Append("LIMIT " + _limit + " ");
            }
            if (_offset > 0)
            {
                output.Append("OFFSET " + _offset);
            }
            if (_bindings != null)
            {
                output.AppendLine();
                output.AppendLine(_bindings.ToString());
            }

            var preOutput = output.ToString();
            if (NamespaceMap.Prefixes.Any())
            {
                foreach (var prefix in NamespaceMap.Prefixes)
                {
                    var uri = NamespaceMap.GetNamespaceUri(prefix).AbsoluteUri;
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
        /// Converts the Query into it's SPARQL Algebra representation (as represented in the Leviathan API).
        /// </summary>
        /// <param name="optimise">Boolean flag indicating whether to apply algebra optimisation.</param>
        /// <returns></returns>
        public ISparqlAlgebra ToAlgebra(bool optimise = true)
        {
            // Depending on how the query gets built we may not have had graph pattern optimization applied
            // which we should do here if query optimization is enabled
            //if (!IsOptimised && optimiseQuery)
            //{
            //    Optimise();
            //}

            // Firstly Transform the Root Graph Pattern to SPARQL Algebra
            ISparqlAlgebra algebra;
            if (_rootGraphPattern != null)
            {
                if (optimise)
                {
                    // If using Algebra Optimisation may use a special algebra in some cases
                    switch (SpecialType)
                    {
                        case SparqlSpecialQueryType.DistinctGraphs:
                            algebra = new SelectDistinctGraphs(Variables.First(v => v.IsResultVariable).Name);
                            break;
                        case SparqlSpecialQueryType.AskAnyTriples:
                            algebra = new AskAnyTriples();
                            break;
                        case SparqlSpecialQueryType.NotApplicable:
                        default:
                            // If not just use the standard transform
                            algebra = _rootGraphPattern.ToAlgebra();
                            break;
                    }
                }
                else
                {
                    // If not using Algebra Optimisation just use the standard transform
                    algebra = _rootGraphPattern.ToAlgebra();
                }
            }
            else
            {
                // No root graph pattern means empty BGP
                algebra = new Bgp();
            }

            // If we have a top level VALUES clause then we'll add it into the algebra here
            if (_bindings != null)
            {
                algebra = Join.CreateJoin(algebra, new Bindings(_bindings));
            }

            // Then we apply any optimisers followed by relevant solution modifiers
            switch (_type)
            {
                case SparqlQueryType.Ask:
                    // Apply Algebra Optimisation is enabled
                    if (optimise)
                    {
                        algebra = ApplyAlgebraOptimisations(algebra);
                    }
                    return new Ask(algebra);

                case SparqlQueryType.Construct:
                case SparqlQueryType.Describe:
                case SparqlQueryType.DescribeAll:
                case SparqlQueryType.Select:
                case SparqlQueryType.SelectAll:
                case SparqlQueryType.SelectAllDistinct:
                case SparqlQueryType.SelectAllReduced:
                case SparqlQueryType.SelectDistinct:
                case SparqlQueryType.SelectReduced:                   
                    // GROUP BY is the first thing applied
                    // This applies if there is a GROUP BY or if there are aggregates
                    // With no GROUP BY it produces a single group of all results
                    if (_groupBy != null || _vars.Any(v => v.IsAggregate))
                    {
                        algebra = new GroupBy(algebra, _groupBy, _vars.Where(v => v.IsAggregate));
                    }

                    // Add HAVING clause immediately after the grouping
                    if (_having != null) algebra = new Having(algebra, _having);

                    // After grouping we do projection
                    // We introduce an Extend for each Project Expression
                    foreach (SparqlVariable var in _vars)
                    {
                        if (var.IsProjection)
                        {
                            algebra = new Extend(algebra, var.Projection, var.Name);
                        }
                    }

                    // We can then Order our results
                    // We do ordering before we do Select but after Project so we can order by any of
                    // the project expressions/aggregates and any variable in the results even if
                    // it won't be output as a result variable
                    if (_orderBy != null) algebra = new OrderBy(algebra, _orderBy);

                    // After Ordering we apply Select
                    // Select effectively trims the results so only result variables are left
                    // This doesn't apply to CONSTRUCT since any variable may be used in the Construct Template
                    // so we don't want to eliminate anything
                    if (_type != SparqlQueryType.Construct)
                    {
                        switch (_type)
                        {
                            case SparqlQueryType.Describe:
                            case SparqlQueryType.Select:
                            case SparqlQueryType.SelectDistinct:
                            case SparqlQueryType.SelectReduced:
                                algebra = new Select(algebra, false, Variables.Where(v => v.IsResultVariable));
                                break;
                            default:
                                algebra = new Select(algebra, true, Variables);
                                break;
                        }
                    }

                    // If we have a Distinct/Reduced then we'll apply those after Selection
                    if (_type == SparqlQueryType.SelectAllDistinct || _type == SparqlQueryType.SelectDistinct)
                    {
                        algebra = new Distinct(algebra);
                    }
                    else if (_type == SparqlQueryType.SelectAllReduced || _type == SparqlQueryType.SelectReduced)
                    {
                        algebra = new Reduced(algebra);
                    }

                    // Finally we can apply any limit and/or offset
                    if (_limit >= 0 || _offset > 0)
                    {
                        algebra = new Slice(algebra, _limit, _offset);
                    }

                    // Apply Algebra Optimisation if enabled
                    if (optimise)
                    {
                        algebra = ApplyAlgebraOptimisations(algebra);
                    }

                    return algebra;

                default:
                    throw new RdfQueryException("Unable to convert unknown Query Types to SPARQL Algebra");
            }
        }

        /// <summary>
        /// Applies Algebra Optimisations to the Query.
        /// </summary>
        /// <param name="algebra">Query Algebra.</param>
        /// <returns>The Query Algebra which may have been transformed to a more optimal form.</returns>
        private ISparqlAlgebra ApplyAlgebraOptimisations(ISparqlAlgebra algebra)
        {
            try
            {
                // Apply Local Optimisers
                foreach (IAlgebraOptimiser opt in _optimisers.Where(o => o.IsApplicable(this)))
                {
                    try
                    {
                        algebra = opt.Optimise(algebra);
                    }
                    catch
                    {
                        // Ignore errors - if an optimiser errors then we leave the algebra unchanged
                    }
                }
                // Apply Global Optimisers
                foreach (IAlgebraOptimiser opt in SparqlOptimiser.AlgebraOptimisers.Where(o => o.IsApplicable(this)))
                {
                    try
                    {
                        algebra = opt.Optimise(algebra);
                    }
                    catch
                    {
                        // Ignore errors - if an optimiser errors then we leave the algebra unchanged
                    }
                }
                return algebra;
            }
            catch
            {
                return algebra;
            }
        }

        /// <summary>
        /// Gets whether the Query's ORDER BY clause can be optimised with Lazy evaluation.
        /// </summary>
        internal bool IsOptimisableOrderBy
        {
            get
            {
                if (_optimisableOrdering == null)
                {
                    if (_orderBy == null)
                    {
                        // If there's no ordering then of course it's optimisable
                        return true;
                    }
                    else
                    {
                        if (_orderBy.IsSimple)
                        {
                            // Is the first pattern a TriplePattern
                            // Do all the Variables occur in the first pattern
                            if (_rootGraphPattern != null)
                            {
                                if (_rootGraphPattern.TriplePatterns.Count > 0)
                                {
                                    if (_rootGraphPattern.TriplePatterns[0].PatternType == TriplePatternType.Match)
                                    {
                                        // If all the Ordering variables occur in the 1st Triple Pattern then we can optimise
                                        _optimisableOrdering = _orderBy.Variables.All(v => _rootGraphPattern.TriplePatterns[0].Variables.Contains(v));
                                    }
                                    else
                                    {
                                        // Not a Triple Pattern as the first pattern in Root Graph Pattern then can't optimise
                                        _optimisableOrdering = false;
                                    }
                                }
                                else
                                {
                                    // Empty Root Graph Pattern => Optimisable
                                    // Like the No Root Graph Pattern case this is somewhat defunct
                                    _optimisableOrdering = true;
                                }
                            }
                            else
                            {
                                // No Root Graph Pattern => Optimisable
                                // Though this is somewhat defunct as Queries without a Root Graph Pattern should
                                // never result in a call to this property
                                _optimisableOrdering = true;
                            }
                        }
                        else
                        {
                            // If the ordering is not simple then it's not optimisable
                            _optimisableOrdering = false;
                        }
                    }
                }
                return (bool)_optimisableOrdering;
            }
        }

        /// <summary>
        /// Gets whether a Query uses the Default Dataset against which it is evaluated.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the value is true then the Query will use whatever dataset is it evaluated against.  If the value is false then the query changes the dataset at one/more points during its evaluation.
        /// </para>
        /// <para>
        /// Things that may change the dataset and cause a query not to use the Default Dataset are as follows:.
        /// <ul>
        ///     <li>FROM clauses (but not FROM NAMED)</li>
        ///     <li>GRAPH clauses</li>
        ///     <li>Subqueries which do not use the default dataset</li>
        /// </ul>
        /// </para>
        /// </remarks>
        public bool UsesDefaultDataset => !_defaultGraphs.Any() && _rootGraphPattern.UsesDefaultDataset;
    }
}
