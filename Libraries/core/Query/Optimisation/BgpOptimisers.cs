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

using VDS.RDF.Query.Algebra;
using VDS.RDF.Update;

namespace VDS.RDF.Query.Optimisation
{
    /// <summary>
    /// An Algebra Optimiser that optimises Algebra to use <see cref="LazyBgp">LazyBgp</see>'s wherever possible
    /// </summary>
    public class LazyBgpOptimiser 
        : BaseAlgebraOptimiser
    {
        /// <summary>
        /// Optimises an Algebra to a form that uses <see cref="LazyBgp">LazyBgp</see> where possible
        /// </summary>
        /// <param name="algebra">Algebra</param>
        /// <param name="depth">Depth</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// By transforming a query to use <see cref="LazyBgp">LazyBgp</see> we can achieve much more efficient processing of some forms of queries
        /// </para>
        /// </remarks>
        protected override ISparqlAlgebra OptimiseInternal(ISparqlAlgebra algebra, int depth)
        {
            try
            {
                ISparqlAlgebra temp;

                //Note this first test is specifically for the default BGP implementation since other optimisers
                //may run before us and replace with other BGP implementations which we don't want to replace hence
                //why we don't check for IBgp here
                if (algebra is Bgp)
                {
                    temp = new LazyBgp(((Bgp)algebra).TriplePatterns);
                }
                //else if (algebra is ILeftJoin)
                //{
                //    ILeftJoin join = (ILeftJoin)algebra;
                //    temp = new LeftJoin(this.OptimiseInternal(join.Lhs, depth + 1), join.Rhs, ((LeftJoin)algebra).Filter);
                //}
                else if (algebra is IUnion)
                {
                    IUnion join = (IUnion)algebra;
                    temp = new LazyUnion(this.OptimiseInternal(join.Lhs, depth + 1), this.OptimiseInternal(join.Rhs, depth + 1));
                }
                else if (algebra is IJoin)
                {
                    IJoin join = (IJoin)algebra;
                    if (join.Lhs.Variables.IsDisjoint(join.Rhs.Variables))
                    {
                        //If the sides of the Join are disjoint then can fully transform the join since we only need to find the requisite number of
                        //solutions on either side to guarantee a product which meets/exceeds the required results
                        //temp = new Join(this.OptimiseInternal(join.Lhs, depth + 1), this.OptimiseInternal(join.Rhs, depth + 1));
                        temp = join.Transform(this);
                    }
                    else
                    {
                        //If the sides are not disjoint then the LHS must be fully evaluated but the RHS need only produce enough
                        //solutions that match
                        //temp = new Join(join.Lhs, this.OptimiseInternal(join.Rhs, depth + 1));
                        temp = join.TransformRhs(this);
                    }
                }
                else if (algebra is Algebra.Graph)
                {
                    //Algebra.Graph g = (Algebra.Graph)algebra;
                    //temp = new Algebra.Graph(this.OptimiseInternal(g.InnerAlgebra, depth + 1), g.GraphSpecifier);
                    IUnaryOperator op = (IUnaryOperator)algebra;
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
                //If the Optimise fails return the current algebra
                return algebra;
            }
        }

        /// <summary>
        /// Determines whether the query can be optimised for lazy evaluation
        /// </summary>
        /// <param name="q">Query</param>
        /// <returns></returns>
        public override bool IsApplicable(SparqlQuery q)
        {
            return q.Limit > 0
                   && !q.HasDistinctModifier
                   && (q.OrderBy == null || q.IsOptimisableOrderBy)
                   && q.GroupBy == null && q.Having == null
                   && q.Bindings == null;
        }

        /// <summary>
        /// Returns that the optimiser does not apply to SPARQL Updates
        /// </summary>
        /// <param name="cmds">Updates</param>
        /// <returns></returns>
        public override bool IsApplicable(SparqlUpdateCommandSet cmds)
        {
            return false;
        }
    }

    /// <summary>
    /// An Algebra Optimiser that optimises Algebra to use <see cref="AskBgp">AskBgp</see>'s wherever possible
    /// </summary>
    public class AskBgpOptimiser
        : BaseAlgebraOptimiser
    {
        /// <summary>
        /// Optimises an Algebra to a form that uses <see cref="AskBgp">AskBgp</see> where possible
        /// </summary>
        /// <param name="algebra">Algebra</param>
        /// <param name="depth">Depth</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// By transforming a query to use <see cref="AskBgp">AskBgp</see> we can achieve much more efficient processing of some forms of queries
        /// </para>
        /// </remarks>
        protected override ISparqlAlgebra OptimiseInternal(ISparqlAlgebra algebra, int depth)
        {
            try
            {
                ISparqlAlgebra temp;
                if (algebra is Bgp)
                {
                    //Bgp is transformed into AskBgp
                    //This tries to find 1 possible solution
                    temp = new AskBgp(((Bgp)algebra).TriplePatterns);
                }
                else if (algebra is ILeftJoin)
                {
                    //LeftJoin is transformed to just be the LHS as the RHS is irrelevant for ASK queries
                    //UNLESS the LeftJoin occurs inside a Filter/Minus BUT we should never get called to transform a 
                    //LeftJoin() for those branches of the algebra as the Optimiseer does not transform 
                    //Filter()/Minus() operators
                    temp = this.OptimiseInternal(((ILeftJoin)algebra).Lhs, depth + 1);
                }
                else if (algebra is IUnion)
                {
                    IUnion join = (IUnion)algebra;
                    temp = new AskUnion(this.OptimiseInternal(join.Lhs, depth + 1), this.OptimiseInternal(join.Rhs, depth + 1));
                }
                else if (algebra is IJoin)
                {
                    IJoin join = (IJoin)algebra;
                    if (join.Lhs.Variables.IsDisjoint(join.Rhs.Variables))
                    {
                        //If the sides of the Join are disjoint then can fully transform the join since we only need to find at least
                        //one solution on either side in order for the query to match
                        //temp = new Join(this.OptimiseInternal(join.Lhs, depth + 1), this.OptimiseInternal(join.Rhs, depth + 1));
                        temp = join.Transform(this);
                    } 
                    else 
                    {
                        //If the sides are not disjoint then the LHS must be fully evaluated but the RHS need only produce at least
                        //one solution based on the full input from the LHS for the query to match
                        //temp = new Join(join.Lhs, this.OptimiseInternal(join.Rhs, depth + 1));
                        temp = join.TransformRhs(this);
                    }
                }
                else if (algebra is Algebra.Graph)
                {
                    //Algebra.Graph g = (Algebra.Graph)algebra;
                    //temp = new Algebra.Graph(this.OptimiseInternal(g.InnerAlgebra, depth + 1), g.GraphSpecifier);
                    IUnaryOperator op = (IUnaryOperator)algebra;
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
                //If the Optimise fails return the current algebra
                return algebra;
            }
        }

        /// <summary>
        /// Determines whether the query can be optimised for ASK evaluation
        /// </summary>
        /// <param name="q">Query</param>
        /// <returns></returns>
        public override bool IsApplicable(SparqlQuery q)
        {
            return q.QueryType == SparqlQueryType.Ask;
        }

        /// <summary>
        /// Returns that the optimiser does not apply to SPARQL Updates
        /// </summary>
        /// <param name="cmds">Updates</param>
        /// <returns></returns>
        public override bool IsApplicable(SparqlUpdateCommandSet cmds)
        {
            return false;
        }
    }
}
