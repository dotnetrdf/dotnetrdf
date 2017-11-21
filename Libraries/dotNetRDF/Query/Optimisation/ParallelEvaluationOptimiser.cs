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

using VDS.RDF.Query.Algebra;
using VDS.RDF.Update;

namespace VDS.RDF.Query.Optimisation
{
    /// <summary>
    /// An Algebra Optimiser which looks for unions and joins that can be evaluated in parallel to improve query evaluation speed in some cases
    /// </summary>
    /// <remarks>
    /// <para>
    /// Using this feature allows you to use experimental parallel SPARQL evaluation optimisations which may improve query evaluation speed for some queries.  A query must either use UNION or have joins which are disjoint in order for any parallel evaluation to take place.
    /// </para>
    /// <para>
    /// Users should be aware that using this optimiser may actually increase evaluation speed in some cases e.g. where either side of a disjoint join will return empty especially when it is the left hand side that will do so.
    /// </para>
    /// <para>
    /// Also note that while use of this optimiser should not cause queries to return incorrect results as it does not change the semantics of the evaluation as it only parallelises independent operators we cannot guarantee that all parallelised queries will return identical results to their non-parallelised counterparts.  If you find a query that you believe is giving incorrect results when used with this optimiser please test without the optimiser enabled to check that the apparent incorrect result is not an artifact of this optimisation.
    /// </para>
    /// </remarks>
    public class ParallelEvaluationOptimiser 
        : IAlgebraOptimiser
    {
        /// <summary>
        /// Optimises the algebra to use parallelised variants of <see cref="Join">Join</see> and <see cref="Union">Union</see> where possible
        /// </summary>
        /// <param name="algebra">Algebra</param>
        /// <returns></returns>
        public ISparqlAlgebra Optimise(ISparqlAlgebra algebra)
        {
            if (algebra is IAbstractJoin)
            {
                if (algebra is Join)
                {
                    Join join = (Join)algebra;
                    if (join.Lhs.Variables.IsDisjoint(join.Rhs.Variables))
                    {
                        return new ParallelJoin(Optimise(join.Lhs), Optimise(join.Rhs));
                    }
                    else
                    {
                        return join.Transform(this);
                    }
                }
                else if (algebra is Union)
                {
                    Union u = (Union)algebra;
                    return new ParallelUnion(Optimise(u.Lhs), Optimise(u.Rhs));
                }
                else
                {
                    return ((IAbstractJoin)algebra).Transform(this);
                }
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

        /// <summary>
        /// Returns that the optimser is applicable to all queries
        /// </summary>
        /// <param name="q">Query</param>
        /// <returns></returns>
        public bool IsApplicable(SparqlQuery q)
        {
            return true;
        }

        /// <summary>
        /// Returns that the optimiser is not applicable to updates
        /// </summary>
        /// <param name="cmds">Updates</param>
        /// <returns></returns>
        public bool IsApplicable(SparqlUpdateCommandSet cmds)
        {
            return false;
        }
    }
}
