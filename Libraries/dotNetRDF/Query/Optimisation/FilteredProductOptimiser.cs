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
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Update;

namespace VDS.RDF.Query.Optimisation
{
    /// <summary>
    /// An Algebra Optimiser which implements the Filtered Product optimisation
    /// </summary>
    /// <remarks>
    /// <para>
    /// A filtered product is implied by any query where there is a product over a join or within a BGP around which there is a Filter which contains variables from both sides of the product.  So rather than computing the entire product and then applying the filter we want to push filter application into the product computation.
    /// </para>
    /// </remarks>
    public class FilteredProductOptimiser
        : IAlgebraOptimiser
    {
        /// <summary>
        /// Optimises the Algebra to use implict joins where applicable
        /// </summary>
        /// <param name="algebra">Algebra</param>
        /// <returns></returns>
        public ISparqlAlgebra Optimise(ISparqlAlgebra algebra)
        {
            try
            {
                if (algebra is Filter)
                {
                    Filter f = (Filter)algebra;

                    // See if the Filtered Product style optimization applies instead
                    int splitPoint = -1;
                    if (f.SparqlFilter.Expression.CanParallelise && IsDisjointOperation(f.InnerAlgebra, f.SparqlFilter.Expression.Variables.ToList(), out splitPoint))
                    {
                        if (splitPoint > -1)
                        {
                            // Means the inner algebra is a BGP we can split into two parts
                            IBgp bgp = (IBgp)f.InnerAlgebra;
                            return new FilteredProduct(new Bgp(bgp.TriplePatterns.Take(splitPoint)), new Bgp(bgp.TriplePatterns.Skip(splitPoint)), f.SparqlFilter.Expression);
                        }
                        else
                        {
                            // Means that the inner algebra is a Join where the sides are disjoint
                            IJoin join = (IJoin)f.InnerAlgebra;
                            return new FilteredProduct(join.Lhs, join.Rhs, f.SparqlFilter.Expression);
                        }
                    }
                    else
                    {
                        return f.Transform(this);
                    }
                }
                else if (algebra is IAbstractJoin)
                {
                    return ((IAbstractJoin)algebra).Transform(this);
                }
                else if (algebra is IUnaryOperator)
                {
                    return ((IUnaryOperator)algebra).Transform(this);
                }
                else
                {
                    return algebra;
                }
            }
            catch
            {
                return algebra;
            }
        }

        private bool IsDisjointOperation(ISparqlAlgebra algebra, List<String> filterVars, out int splitPoint)
        {
            splitPoint = -1;
            if (algebra is IBgp)
            {
                // Get Triple Patterns, can't split into a product if there are blank variables present
                List<ITriplePattern> ps = ((IBgp)algebra).TriplePatterns.ToList();
                if (ps.Any(p => !p.HasNoBlankVariables)) return false;

                // Iterate over the Triple Patterns to see if we can split into a Product
                List<String> vars = new List<String>();
                for (int i = 0; i < ps.Count; i++)
                {
                    // Not a product if we've seen both variables already
                    if (filterVars.All(v => vars.Contains(v))) return false;

                    ITriplePattern p = ps[i];
                    if (p.PatternType == TriplePatternType.Match || p.PatternType == TriplePatternType.SubQuery)
                    {
                        if (vars.Count > 0 && vars.IsDisjoint(p.Variables))
                        {
                            // Is a filterable product if we've not seen all the variables so far and have hit a point where a product occurs
                            // and all the variables are not in the RHS
                            Bgp rhs = new Bgp(ps.Skip(i));
                            if (!filterVars.All(v => rhs.Variables.Contains(v)))
                            {
                                splitPoint = i;
                                return true;
                            }
                        }
                        vars.AddRange(p.Variables);
                    }
                    else if (p.PatternType == TriplePatternType.BindAssignment || p.PatternType == TriplePatternType.LetAssignment)
                    {
                        vars.Add(((IAssignmentPattern)p).VariableName);
                    }
                    else if (p.PatternType == TriplePatternType.Filter)
                    {
                        continue;
                    }
                    else
                    {
                        return false;
                    }
                }
                // If we get all the way here then not a product
                return false;
            }
            else if (algebra is IJoin)
            {
                IJoin join = (IJoin)algebra;
                if (join.Lhs.Variables.IsDisjoint(join.Rhs.Variables))
                {
                    // There a product between the two sides of the join but are the variables spead over different sides of that join?
                    // If all variables occur on one side then this is not a filtered product
                    return !filterVars.All(v => join.Lhs.Variables.Contains(v)) && !filterVars.All(v => join.Rhs.Variables.Contains(v));
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Returns that this optimiser is applicable to all queries
        /// </summary>
        /// <param name="q">Query</param>
        /// <returns></returns>
        public bool IsApplicable(SparqlQuery q)
        {
            return true;
        }

        /// <summary>
        /// Returns that this optimiser is applicable to all updates
        /// </summary>
        /// <param name="cmds">Updates</param>
        /// <returns></returns>
        public bool IsApplicable(SparqlUpdateCommandSet cmds)
        {
            return true;
        }
    }
}
