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

using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.Common.Collections;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Expressions.Primary;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Expressions.Functions.Sparql.Boolean
{
    /// <summary>
    /// Represents an EXIST/NOT EXISTS clause used as a Function in an Expression
    /// </summary>
    public class ExistsFunction 
        : ISparqlExpression
    {
        private GraphPattern _pattern;
        private bool _mustExist;

        private BaseMultiset _result;
        private int? _lastInput;
        private int _lastCount = 0;
        private List<System.String> _joinVars;
        private HashSet<int> _exists;

        /// <summary>
        /// Creates a new EXISTS/NOT EXISTS function
        /// </summary>
        /// <param name="pattern">Graph Pattern</param>
        /// <param name="mustExist">Whether this is an EXIST</param>
        public ExistsFunction(GraphPattern pattern, bool mustExist)
        {
            _pattern = pattern;
            _mustExist = mustExist;
        }

        /// <summary>
        /// Gets the Value of this function which is a Boolean as a Literal Node
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            if (_result == null || _lastInput == null || (int)_lastInput != context.InputMultiset.GetHashCode() || _lastCount != context.InputMultiset.Count) EvaluateInternal(context);

            if (_mustExist)
            {
                // If an EXISTS then Null/Empty Other results in false
                if (_result is NullMultiset) return new BooleanNode(null, false);
                if (_result.IsEmpty) return new BooleanNode(null, false);
            }
            else
            {
                // If a NOT EXISTS then Null/Empty results in true
                if (_result is NullMultiset) return new BooleanNode(null, true);
                if (_result.IsEmpty) return new BooleanNode(null, true);
            }

            if (_joinVars.Count == 0)
            {
                // If Disjoint then all solutions are compatible
                if (_mustExist)
                {
                    // If Disjoint and must exist then true since
                    return new BooleanNode(null, true);
                }
                else
                {
                    // If Disjoint and must not exist then false
                    return new BooleanNode(null, false);
                }
            }

            ISet x = context.InputMultiset[bindingID];

            bool exists = _exists.Contains(x.ID);
            if (_mustExist)
            {
                // If an EXISTS then return the value of exists i.e. are there any compatible solutions
                return new BooleanNode(null, exists);
            }
            else
            {
                // If a NOT EXISTS then return the negation of exists i.e. if compatible solutions exist then we must return false, if none we return true
                return new BooleanNode(null, !exists);
            }
        }

        /// <summary>
        /// Internal method which evaluates the Graph Pattern
        /// </summary>
        /// <param name="origContext">Evaluation Context</param>
        /// <remarks>
        /// We only ever need to evaluate the Graph Pattern once to get the Results
        /// </remarks>
        private void EvaluateInternal(SparqlEvaluationContext origContext)
        {
            _result = null;

            // We must take a copy of the original context as otherwise we can have strange results
            SparqlEvaluationContext context = new SparqlEvaluationContext(origContext.Query, origContext.Data);
            context.InputMultiset = origContext.InputMultiset;
            context.OutputMultiset = new Multiset();
            _lastInput = context.InputMultiset.GetHashCode();
            _lastCount = context.InputMultiset.Count;

            // REQ: Optimise the algebra here
            ISparqlAlgebra existsClause = _pattern.ToAlgebra();
            _result = context.Evaluate(existsClause);

            // This is the new algorithm which is also correct but is O(3n) so much faster and scalable
            // Downside is that it does require more memory than the old algorithm
            _joinVars = origContext.InputMultiset.Variables.Where(v => _result.Variables.Contains(v)).ToList();
            if (_joinVars.Count == 0) return;

            List<MultiDictionary<INode, List<int>>> values = new List<MultiDictionary<INode, List<int>>>();
            List<List<int>> nulls = new List<List<int>>();
            foreach (System.String var in _joinVars)
            {
                values.Add(new MultiDictionary<INode, List<int>>(new FastVirtualNodeComparer()));
                nulls.Add(new List<int>());
            }

            // First do a pass over the LHS Result to find all possible values for joined variables
            foreach (ISet x in origContext.InputMultiset.Sets)
            {
                int i = 0;
                foreach (System.String var in _joinVars)
                {
                    INode value = x[var];
                    if (value != null)
                    {
                        List<int> ids;
                        if (values[i].TryGetValue(value, out ids))
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
            _exists = new HashSet<int>();
            foreach (ISet y in _result.Sets)
            {
                IEnumerable<int> possMatches = null;
                int i = 0;
                foreach (System.String var in _joinVars)
                {
                    INode value = y[var];
                    if (value != null)
                    {
                        if (values[i].ContainsKey(value))
                        {
                            possMatches = (possMatches == null ? values[i][value].Concat(nulls[i]) : possMatches.Intersect(values[i][value].Concat(nulls[i])));
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
                        possMatches = (possMatches == null ? origContext.InputMultiset.SetIDs : possMatches.Intersect(origContext.InputMultiset.SetIDs));
                    }
                    i++;
                }
                if (possMatches == null) continue;

                // Look at possible matches, if is a valid match then mark the set as having an existing match
                // Don't reconsider sets which have already been marked as having an existing match
                foreach (int poss in possMatches)
                {
                    if (_exists.Contains(poss)) continue;
                    if (origContext.InputMultiset[poss].IsCompatibleWith(y, _joinVars))
                    {
                        _exists.Add(poss);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the Variables used in this Expression
        /// </summary>
        public IEnumerable<string> Variables
        {
            get 
            { 
                return (from p in _pattern.TriplePatterns
                        from v in p.Variables
                        select v).Distinct();
            }
        }

        /// <summary>
        /// Gets whether an expression can safely be evaluated in parallel
        /// </summary>
        public virtual bool CanParallelise
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the String representation of the Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            if (_mustExist)
            {
                output.Append("EXISTS ");
            }
            else
            {
                output.Append("NOT EXISTS ");
            }
            output.Append(_pattern.ToString());
            return output.ToString();
        }

        /// <summary>
        /// Gets the Type of the Expression
        /// </summary>
        public SparqlExpressionType Type
        {
            get
            {
                return SparqlExpressionType.GraphOperator;
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public string Functor
        {
            get
            {
                if (_mustExist)
                {
                    return SparqlSpecsHelper.SparqlKeywordExists;
                }
                else
                {
                    return SparqlSpecsHelper.SparqlKeywordNotExists;
                }
            }
        }

        /// <summary>
        /// Gets the Arguments of the Expression
        /// </summary>
        public IEnumerable<ISparqlExpression> Arguments
        {
            get
            {
                return new ISparqlExpression[] { new GraphPatternTerm(_pattern) };
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            ISparqlExpression temp = transformer.Transform(new GraphPatternTerm(_pattern));
            if (temp is GraphPatternTerm)
            {
                return new ExistsFunction(((GraphPatternTerm)temp).Pattern, _mustExist);
            }
            else
            {
                throw new RdfQueryException("Unable to transform an EXISTS/NOT EXISTS function since the expression transformer in use failed to transform the inner Graph Pattern Expression to another Graph Pattern Expression");
            }
        }
    }
}
