/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2025 dotNetRDF Project (http://dotnetrdf.org/)
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
using VDS.Common.Collections;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Functions.Sparql.Boolean;
using VDS.RDF.Query.Expressions.Functions.Sparql.Constructor;
using VDS.RDF.Query.Expressions.Functions.Sparql.Numeric;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query;

/// <summary>
/// Extends the <see cref="BaseExpressionProcessor{TContext,TBinding}"/> with the logic for retrieving the bound value for a specific binding in a <see cref="SparqlEvaluationContext"/>.
/// </summary>
internal class LeviathanExpressionProcessor : BaseExpressionProcessor<SparqlEvaluationContext, int>
{
    private readonly LeviathanQueryProcessor _algebraProcessor;
    private readonly Dictionary<ISparqlExpression, ExistsCacheEntry> _existsCache =
        new Dictionary<ISparqlExpression, ExistsCacheEntry>();

    private readonly Dictionary<ISparqlExpression, Dictionary<int, IValuedNode>> _randCache =
        new Dictionary<ISparqlExpression, Dictionary<int, IValuedNode>>();

    private readonly Random _rnd = new Random();


    public LeviathanExpressionProcessor(LeviathanQueryOptions options, LeviathanQueryProcessor algebraProcessor):base(
        options.NodeComparer, options.UriFactory, options.StrictOperators)
    {
        _algebraProcessor = algebraProcessor;
        AggregateProcessor = new LeviathanAggregateProcessor(this);
    }

    public ISparqlAggregateProcessor<IValuedNode, SparqlEvaluationContext, int> AggregateProcessor { get; }

    protected override IValuedNode GetBoundValue(string variableName, SparqlEvaluationContext context, int binding)
    {
        INode value = context.Binder.Value(variableName, binding);
        return value.AsValuedNode();
    }

    protected override IEnumerable<ISparqlCustomExpressionFactory> GetExpressionFactories(SparqlEvaluationContext context)
    {
        return context.Query?.ExpressionFactories;
    }

    public override IValuedNode ProcessAggregateTerm(AggregateTerm aggregate, SparqlEvaluationContext context, int binding)
    {
        {
            IValuedNode aggValue;
            if (context.Binder.IsGroup(binding))
            {
                BindingGroup group = context.Binder.Group(binding);
                context.Binder.SetGroupContext(true);
                aggValue = aggregate.Aggregate.Accept(AggregateProcessor, context, group.BindingIDs);
                context.Binder.SetGroupContext(false);
            }
            else
            {
                aggValue = aggregate.Aggregate.Accept(AggregateProcessor, context, context.InputMultiset.SetIDs);
            }
            return aggValue;
        }
    }

    private class ExistsCacheEntry
    {
        public int InputHashCode { get; set; }
        public int InputCount { get; set; }
        public bool ResultNullOrEmpty { get; set; }
        public List<string> JoinVars { get; set; }
        public HashSet<int> Bindings { get; set; }
    }

    public override IValuedNode ProcessExistsFunction(ExistsFunction exists, SparqlEvaluationContext context, int binding)
    {
        if (!_existsCache.TryGetValue(exists, out ExistsCacheEntry cacheEntry) ||
            cacheEntry.InputHashCode != context.InputMultiset.GetHashCode() ||
            cacheEntry.InputCount != context.InputMultiset.Count
        )
        {
            cacheEntry = EvaluateExistsPattern(exists, context);
            _existsCache[exists] = cacheEntry;
        }

        if (exists.MustExist)
        {
            // If an EXISTS then Null/Empty Other results in false
            if (cacheEntry.ResultNullOrEmpty) return new BooleanNode(false);
        }
        else
        {
            // If a NOT EXISTS then Null/Empty results in true
            if (cacheEntry.ResultNullOrEmpty) return new BooleanNode(true);
        }

        if (cacheEntry.JoinVars.Count == 0)
        {
            // If Disjoint then all solutions are compatible
            return exists.MustExist ? new BooleanNode(true) : new BooleanNode(false);
        }

        ISet x = context.InputMultiset[binding];

        var found = cacheEntry.Bindings.Contains(x.ID);
        return exists.MustExist ? new BooleanNode(found) : new BooleanNode(!found);
    }

    private ExistsCacheEntry EvaluateExistsPattern(ExistsFunction exists, SparqlEvaluationContext origContext)
    {
        // We must take a copy of the original context as otherwise we can have strange results
        var context = new SparqlEvaluationContext(origContext.Query, origContext.Data, origContext.Options)
        {
            InputMultiset = origContext.InputMultiset,
            OutputMultiset = new Multiset(),
        };
        var cacheEntry = new ExistsCacheEntry
        {
            InputHashCode = context.InputMultiset.GetHashCode(), InputCount = context.InputMultiset.Count,
        };

        // REQ: Optimise the algebra here
        ISparqlAlgebra existsClause = exists.Pattern.ToAlgebra();
        BaseMultiset result = existsClause.Accept(_algebraProcessor, context);
        cacheEntry.ResultNullOrEmpty = result.IsEmpty;

        // This is the new algorithm which is also correct but is O(3n) so much faster and scalable
        // Downside is that it does require more memory than the old algorithm
        var joinVars = origContext.InputMultiset.Variables.Where(v => result.Variables.Contains(v)).ToList();
        cacheEntry.JoinVars = joinVars;
        if (joinVars.Count == 0) return cacheEntry;

        var values = new List<MultiDictionary<INode, List<int>>>();
        var nulls = new List<List<int>>();
        foreach (var var in joinVars)
        {
            values.Add(new MultiDictionary<INode, List<int>>(new FastVirtualNodeComparer()));
            nulls.Add(new List<int>());
        }

        // First do a pass over the LHS Result to find all possible values for joined variables
        foreach (ISet x in origContext.InputMultiset.Sets)
        {
            var i = 0;
            foreach (var var in joinVars)
            {
                INode value = x[var];
                if (value != null)
                {
                    if (values[i].TryGetValue(value, out List<int> ids))
                    {
                        ids.Add(x.ID);
                    }
                    else
                    {
                        values[i].Add(value, new List<int> { x.ID });
                    }
                }
                else
                {
                    nulls[i].Add(x.ID);
                }
                i++;
            }
        }


        // Then do a pass over the RHS and work out the intersections
        cacheEntry.Bindings = new HashSet<int>();
        foreach (ISet y in result.Sets)
        {
            IEnumerable<int> possMatches = null;
            var i = 0;
            foreach (var var in joinVars)
            {
                INode value = y[var];
                if (value != null)
                {
                    if (values[i].ContainsKey(value))
                    {
                        possMatches = possMatches == null ? values[i][value].Concat(nulls[i]) : possMatches.Intersect(values[i][value].Concat(nulls[i]));
                    }
                    else
                    {
                        possMatches = Enumerable.Empty<int>();
                        break;
                    }
                }
                else
                {
                    // Don't forget that a null will be potentially compatible with everything
                    possMatches = possMatches == null ? origContext.InputMultiset.SetIDs : possMatches.Intersect(origContext.InputMultiset.SetIDs);
                }
                i++;
            }
            if (possMatches == null) continue;

            // Look at possible matches, if is a valid match then mark the set as having an existing match
            // Don't reconsider sets which have already been marked as having an existing match
            foreach (var poss in possMatches)
            {
                if (cacheEntry.Bindings.Contains(poss)) continue;
                if (origContext.InputMultiset[poss].IsCompatibleWith(y, joinVars))
                {
                    cacheEntry.Bindings.Add(poss);
                }
            }
        }

        return cacheEntry;
    }

    public override IValuedNode ProcessBNodeFunction(BNodeFunction bNode, SparqlEvaluationContext context, int binding)
    {
        {
            var funcContext = context[SparqlSpecsHelper.SparqlKeywordBNode] as BNodeFunctionContext;

            if (funcContext == null)
            {
                funcContext = new BNodeFunctionContext(context.InputMultiset.GetHashCode());
                context[SparqlSpecsHelper.SparqlKeywordBNode] = funcContext;
            }
            else if (funcContext.CurrentInput != context.InputMultiset.GetHashCode())
            {
                // Clear the Context
                funcContext.BlankNodes.Clear();
                context[SparqlSpecsHelper.SparqlKeywordBNode] = funcContext;
            }

            if (bNode.InnerExpression == null)
            {
                // If no argument then always a fresh BNode
                return funcContext.Graph.CreateBlankNode().AsValuedNode();
            }
            else
            {
                INode temp = bNode.InnerExpression.Accept(this, context, binding);
                if (temp != null)
                {
                    if (temp.NodeType == NodeType.Literal)
                    {
                        var lit = (ILiteralNode)temp;

                        if (lit.DataType == null)
                        {
                            if (lit.Language.Equals(string.Empty))
                            {
                                if (!funcContext.BlankNodes.ContainsKey(binding))
                                {
                                    funcContext.BlankNodes.Add(binding, new Dictionary<string, INode>());
                                }

                                if (!funcContext.BlankNodes[binding].ContainsKey(lit.Value))
                                {
                                    funcContext.BlankNodes[binding].Add(lit.Value, funcContext.Graph.CreateBlankNode());
                                }
                                return funcContext.BlankNodes[binding][lit.Value].AsValuedNode();
                            }
                            else
                            {
                                throw new RdfQueryException("Cannot create a Blank Node when the argument Expression evaluates to a lanuage specified literal");
                            }
                        }
                        else
                        {
                            throw new RdfQueryException("Cannot create a Blank Node when the argument Expression evaluates to a typed literal node");
                        }
                    }
                    else
                    {
                        throw new RdfQueryException("Cannot create a Blank Node when the argument Expression evaluates to a non-literal node");
                    }
                }
                else
                {
                    throw new RdfQueryException("Cannot create a Blank Node when the argument Expression evaluates to null");
                }
            }
        }
    }

    class BNodeFunctionContext
    {
        private Dictionary<int, Dictionary<string, INode>> _bnodes = new Dictionary<int, Dictionary<string, INode>>();
        private Graph _g = new Graph();
        private int _currInput;

        public BNodeFunctionContext(int currInput)
        {
            _currInput = currInput;
        }

        public int CurrentInput
        {
            get
            {
                return _currInput;
            }
        }

        public IGraph Graph
        {
            get
            {
                return _g;
            }
        }

        public Dictionary<int, Dictionary<string, INode>> BlankNodes
        {
            get
            {
                return _bnodes;
            }
            set
            {
                _bnodes = value;
            }
        }
    }

    public override IValuedNode ProcessIriFunction(IriFunction iri, SparqlEvaluationContext context, int binding)
    {
        {
            IValuedNode result = iri.InnerExpression.Accept(this, context, binding);
            if (result == null)
            {
                throw new RdfQueryException("Cannot create an IRI from a null");
            }
            else
            {
                switch (result.NodeType)
                {
                    case NodeType.Literal:
                        var lit = (ILiteralNode)result;
                        var baseUri = string.Empty;
                        if (context.Query != null) baseUri = context.Query.BaseUri.ToSafeString();
                        string uri;
                        if (lit.DataType == null)
                        {
                            uri = Tools.ResolveUri(lit.Value, baseUri);
                            return new UriNode(context.UriFactory.Create(uri));
                        }
                        else
                        {
                            var dt = lit.DataType.AbsoluteUri;
                            if (dt.Equals(XmlSpecsHelper.XmlSchemaDataTypeString, StringComparison.Ordinal))
                            {
                                uri = Tools.ResolveUri(lit.Value, baseUri);
                                return new UriNode(context.UriFactory.Create(uri));
                            }
                            else
                            {
                                throw new RdfQueryException("Cannot create an IRI from a non-string typed literal");
                            }
                        }

                    case NodeType.Uri:
                        // Already a URI so nothing to do
                        return result;
                    default:
                        throw new RdfQueryException("Cannot create an IRI from a non-URI/String literal");
                }
            }
        }

    }

    public override IValuedNode ProcessRandFunction(RandFunction rand, SparqlEvaluationContext context, int binding)
    {
        // Ensure we return a consistent value when evaluating a set we have already seen
        Dictionary<int, IValuedNode> randExprCache;
        lock (_randCache)
        {
            if (!_randCache.TryGetValue(rand, out randExprCache))
            {
                randExprCache = new Dictionary<int, IValuedNode>();
                _randCache[rand] = randExprCache;
            }
        }

        lock (rand)
        {
            if (randExprCache.TryGetValue(binding, out IValuedNode result)) return result;
            result = new DoubleNode(_rnd.NextDouble());
            randExprCache[binding] = result;
            return result;
        }
    }
}