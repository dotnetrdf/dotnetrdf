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
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra
{
    /// <summary>
    /// Represents the Slice Operation in the SPARQL Algebra
    /// </summary>
    public class Slice
        : IUnaryOperator
    {
        private readonly ISparqlAlgebra _pattern;
        private readonly int _limit = -1, _offset = 0;
        private readonly bool _detectSettings = true;

        /// <summary>
        /// Creates a new Slice modifier which will detect LIMIT and OFFSET from the query
        /// </summary>
        /// <param name="pattern">Pattern</param>
        public Slice(ISparqlAlgebra pattern)
        {
            _pattern = pattern;
        }

        /// <summary>
        /// Creates a new Slice modifier which uses a specific LIMIT and OFFSET
        /// </summary>
        /// <param name="pattern">Pattern</param>
        /// <param name="limit">Limit</param>
        /// <param name="offset">Offset</param>
        public Slice(ISparqlAlgebra pattern, int limit, int offset)
            : this(pattern)
        {
            _limit = Math.Max(-1, limit);
            _offset = Math.Max(0, offset);
            _detectSettings = false;
        }

        /// <summary>
        /// Evaluates the Slice by applying the appropriate LIMIT and OFFSET to the Results
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        public BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            // Detect the Offset and Limit from the Query if required
            int limit = _limit, offset = _offset;
            if (_detectSettings)
            {
                if (context.Query != null)
                {
                    limit = Math.Max(-1, context.Query.Limit);
                    offset = Math.Max(0, context.Query.Offset);
                }
            }

            // First check what variables are present, we need this if we have to create
            // a new multiset
            IEnumerable<String> vars;
            if (context.InputMultiset is IdentityMultiset || context.InputMultiset is NullMultiset)
            {
                vars = (context.Query != null) ? context.Query.Variables.Where(v => v.IsResultVariable).Select(v => v.Name) : context.InputMultiset.Variables;
            }
            else
            {
                vars = context.InputMultiset.Variables;
            }

            if (limit == 0)
            {
                // If Limit is Zero we can skip evaluation
                context.OutputMultiset = new Multiset(vars);
                return context.OutputMultiset;
            }
            else
            {
                // Otherwise we have a limit/offset to apply

                // Firstly evaluate the inner algebra
                context.InputMultiset = context.Evaluate(_pattern);
                context.InputMultiset.VirtualCount = context.InputMultiset.Count;
                // Then apply the offset
                if (offset > 0)
                {
                    if (offset > context.InputMultiset.Count)
                    {
                        // If the Offset is greater than the count return nothing
                        context.OutputMultiset = new Multiset(vars);
                        return context.OutputMultiset;
                    }
                    else
                    {
                        // Otherwise discard the relevant number of Bindings
                        foreach (int id in context.InputMultiset.SetIDs.Take(offset).ToList())
                        {
                            context.InputMultiset.Remove(id);
                        }
                    }
                }
                // Finally apply the limit
                if (limit > 0)
                {
                    if (context.InputMultiset.Count > limit)
                    {
                        // If the number of results is greater than the limit remove the extraneous
                        // results
                        foreach (int id in context.InputMultiset.SetIDs.Skip(limit).ToList())
                        {
                            context.InputMultiset.Remove(id);
                        }
                    }
                }

                // Then return the result
                context.OutputMultiset = context.InputMultiset;
                return context.OutputMultiset;
            }
        }

        /// <summary>
        /// Gets the Variables used in the Algebra
        /// </summary>
        public IEnumerable<String> Variables
        {
            get
            {
                return _pattern.Variables.Distinct();
            }
        }

        /// <summary>
        /// Gets the enumeration of floating variables in the algebra i.e. variables that are not guaranteed to have a bound value
        /// </summary>
        public IEnumerable<String> FloatingVariables { get { return _pattern.FloatingVariables; } }

        /// <summary>
        /// Gets the enumeration of fixed variables in the algebra i.e. variables that are guaranteed to have a bound value
        /// </summary>
        public IEnumerable<String> FixedVariables { get { return _pattern.FixedVariables; } }

        /// <summary>
        /// Gets the Limit in use (-1 indicates no Limit)
        /// </summary>
        public int Limit
        {
            get
            {
                return _limit;
            }
        }

        /// <summary>
        /// Gets the Offset in use (0 indicates no Offset)
        /// </summary>
        public int Offset
        {
            get
            {
                return _offset;
            }
        }

        /// <summary>
        /// Gets whether the Algebra will detect the Limit and Offset to use from the provided query
        /// </summary>
        public bool DetectFromQuery
        {
            get
            {
                return _detectSettings;
            }
        }

        /// <summary>
        /// Gets the Inner Algebra
        /// </summary>
        public ISparqlAlgebra InnerAlgebra
        {
            get
            {
                return _pattern;
            }
        }

        /// <summary>
        /// Gets the String representation of the Algebra
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Slice(" + _pattern.ToString() + ", LIMIT " + _limit + ", OFFSET " + _offset + ")";
        }

        /// <summary>
        /// Converts the Algebra back to a SPARQL Query
        /// </summary>
        /// <returns></returns>
        public SparqlQuery ToQuery()
        {
            SparqlQuery q = _pattern.ToQuery();
            q.Limit = _limit;
            q.Offset = _offset;
            return q;
        }

        /// <summary>
        /// Throws an exception since a Slice() cannot be converted back to a Graph Pattern
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">Thrown since a Slice() cannot be converted to a Graph Pattern</exception>
        public GraphPattern ToGraphPattern()
        {
            throw new NotSupportedException("A Slice() cannot be converted to a Graph Pattern");
        }

        /// <summary>
        /// Transforms the Inner Algebra using the given Optimiser
        /// </summary>
        /// <param name="optimiser">Optimiser</param>
        /// <returns></returns>
        public ISparqlAlgebra Transform(IAlgebraOptimiser optimiser)
        {
            return new Slice(optimiser.Optimise(_pattern), _limit, _offset);
        }
    }
}
