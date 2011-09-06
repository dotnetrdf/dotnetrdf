/*

Copyright Robert Vesse 2009-11
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
                        return new ParallelJoin(this.Optimise(join.Lhs), this.Optimise(join.Rhs));
                    }
                    else
                    {
                        return join.Transform(this);
                    }
                }
                else if (algebra is Union)
                {
                    Union u = (Union)algebra;
                    return new ParallelUnion(this.Optimise(u.Lhs), this.Optimise(u.Rhs));
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
