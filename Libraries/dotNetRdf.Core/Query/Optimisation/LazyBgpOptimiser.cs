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

// unset

using VDS.RDF.Query.Algebra;
using VDS.RDF.Update;

namespace VDS.RDF.Query.Optimisation;

/// <summary>
/// An Algebra Optimiser that optimises Algebra to use <see cref="LazyBgp">LazyBgp</see>'s wherever possible.
/// </summary>
public class LazyBgpOptimiser 
    : BaseAlgebraOptimiser
{
    /// <summary>
    /// Optimises an Algebra to a form that uses <see cref="LazyBgp">LazyBgp</see> where possible.
    /// </summary>
    /// <param name="algebra">Algebra.</param>
    /// <param name="depth">Depth.</param>
    /// <returns></returns>
    /// <remarks>
    /// <para>
    /// By transforming a query to use <see cref="LazyBgp">LazyBgp</see> we can achieve much more efficient processing of some forms of queries.
    /// </para>
    /// </remarks>
    protected override ISparqlAlgebra OptimiseInternal(ISparqlAlgebra algebra, int depth)
    {
        try
        {
            ISparqlAlgebra temp;

            // Note this first test is specifically for the default BGP implementation since other optimisers
            // may run before us and replace with other BGP implementations which we don't want to replace hence
            // why we don't check for IBgp here
            if (algebra is Bgp)
            {
                temp = new LazyBgp(((Bgp)algebra).TriplePatterns);
            }
            else if (algebra is IUnion)
            {
                var join = (IUnion)algebra;
                temp = new LazyUnion(OptimiseInternal(join.Lhs, depth + 1), OptimiseInternal(join.Rhs, depth + 1));
            }
            else if (algebra is IJoin)
            {
                var join = (IJoin)algebra;
                if (join.Lhs.Variables.IsDisjoint(join.Rhs.Variables))
                {
                    // If the sides of the Join are disjoint then can fully transform the join since we only need to find the requisite number of
                    // solutions on either side to guarantee a product which meets/exceeds the required results
                    temp = join.Transform(this);
                }
                else
                {
                    // If the sides are not disjoint then the LHS must be fully evaluated but the RHS need only produce enough
                    // solutions that match
                    temp = join.TransformRhs(this);
                }
            }
            else if (algebra is Algebra.Graph || algebra is Select || algebra is Slice || algebra is OrderBy)
            {
                var op = (IUnaryOperator)algebra;
                temp = op.Transform(this);
            }
            else
            {
                temp = algebra;
            }
            return temp;
        }
        catch
        {
            // If the Optimise fails return the current algebra
            return algebra;
        }
    }

    /// <summary>
    /// Determines whether the query can be optimised for lazy evaluation.
    /// </summary>
    /// <param name="q">Query.</param>
    /// <returns></returns>
    public override bool IsApplicable(SparqlQuery q)
    {
        return q.Limit > 0
               && !q.HasDistinctModifier
               && (q.OrderBy == null || q.IsOptimisableOrderBy)
               && q.GroupBy == null && q.Having == null
               && !q.IsAggregate
               && q.Bindings == null;
    }

    /// <summary>
    /// Returns that the optimiser does not apply to SPARQL Updates.
    /// </summary>
    /// <param name="cmds">Updates.</param>
    /// <returns></returns>
    public override bool IsApplicable(SparqlUpdateCommandSet cmds)
    {
        return false;
    }
}