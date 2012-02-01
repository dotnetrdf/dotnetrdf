using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.Common;
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra
{
    public class FastJoin
        : IJoin
    {
        private ISparqlAlgebra _lhs, _rhs;

        public FastJoin(ISparqlAlgebra lhs, ISparqlAlgebra rhs)
        {
            this._lhs = lhs;
            this._rhs = rhs;
        }

        /// <summary>
        /// Evalutes a Join
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        public BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            BaseMultiset initialInput = context.InputMultiset;
            BaseMultiset lhsResult = context.Evaluate(this._lhs);
            context.CheckTimeout();

            if (lhsResult is NullMultiset)
            {
                context.OutputMultiset = lhsResult;
            }
            else if (lhsResult.IsEmpty)
            {
                context.OutputMultiset = new NullMultiset();
            }
            else
            {
                //Only Execute the RHS if the LHS has some results
                context.InputMultiset = lhsResult;
                BaseMultiset rhsResult = context.Evaluate(this._rhs);
                context.CheckTimeout();

                //If the Other is the Identity Multiset the result is this Multiset
                if (rhsResult is IdentityMultiset) return lhsResult;
                //If the Other is the Null Multiset the result is the Null Multiset
                if (rhsResult is NullMultiset) return rhsResult;
                //If the Other is Empty then the result is the Null Multiset
                if (rhsResult.IsEmpty) return new NullMultiset();

                //Find the First Variable from this Multiset which is in both Multisets
                //If there is no Variable from this Multiset in the other Multiset then this
                //should be a Join operation instead of a LeftJoin
                List<String> joinVars = lhsResult.Variables.Where(v => rhsResult.Variables.Contains(v)).ToList();
                if (joinVars.Count == 0) return lhsResult.Product(rhsResult);

                //Start building the Joined Set
                context.OutputMultiset = new Multiset();
                List<HashTable<INode, int>> values = new List<HashTable<INode, int>>();
                foreach (String var in joinVars)
                {
                    values.Add(new HashTable<INode, int>(HashTableBias.Enumeration));
                }

                //First do a pass over the LHS Result to find all possible values for joined variables
                foreach (Set x in lhsResult.Sets)
                {
                    int i = 0;
                    foreach (String var in joinVars)
                    {
                        INode value = x[var];
                        if (value != null)
                        {
                            values[i].Add(value, x.ID);
                        }
                        i++;
                    }
                }

                //Then do a pass over the RHS and work out the intersections
                foreach (Set y in rhsResult.Sets)
                {
                    IEnumerable<int> possMatches = null;
                    int i = 0;
                    foreach (String var in joinVars)
                    {
                        INode value = y[var];
                        if (value != null)
                        {
                            if (values[i].ContainsKey(value))
                            {
                                possMatches = (possMatches == null ? values[i].GetValues(value) : possMatches.Intersect(values[i].GetValues(value)));
                            }
                            else
                            {
                                possMatches = Enumerable.Empty<int>();
                                break;
                            }
                        }
                        else
                        {
                            //Don't forget that a null will be potentially compatible with everything
                            possMatches = (possMatches == null ? lhsResult.SetIDs : possMatches.Intersect(lhsResult.SetIDs));
                        }
                        i++;
                    }
                    if (possMatches == null) continue;

                    //Now do the actual joins for the current set
                    foreach (int poss in possMatches)
                    {
                        if (lhsResult[poss].IsCompatibleWith(y, joinVars))
                        {
                            context.OutputMultiset.Add(lhsResult[poss].Join(y));
                        }
                    }
                }
            }

            context.InputMultiset = context.OutputMultiset;
            return context.OutputMultiset;
        }

        /// <summary>
        /// Gets the Variables used in the Algebra
        /// </summary>
        public IEnumerable<String> Variables
        {
            get
            {
                return (this._lhs.Variables.Concat(this._rhs.Variables)).Distinct();
            }
        }

        /// <summary>
        /// Gets the LHS of the Join
        /// </summary>
        public ISparqlAlgebra Lhs
        {
            get
            {
                return this._lhs;
            }
        }

        /// <summary>
        /// Gets the RHS of the Join
        /// </summary>
        public ISparqlAlgebra Rhs
        {
            get
            {
                return this._rhs;
            }
        }

        /// <summary>
        /// Gets the String representation of the Join
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "FastJoin(" + this._lhs.ToString() + ", " + this._rhs.ToString() + ")";
        }

        /// <summary>
        /// Converts the Algebra back to a SPARQL Query
        /// </summary>
        /// <returns></returns>
        public SparqlQuery ToQuery()
        {
            SparqlQuery q = new SparqlQuery();
            q.RootGraphPattern = this.ToGraphPattern();
            q.Optimise();
            return q;
        }

        /// <summary>
        /// Converts the Algebra back to a SPARQL Query
        /// </summary>
        /// <returns></returns>
        public GraphPattern ToGraphPattern()
        {
            GraphPattern p = this._lhs.ToGraphPattern();
            p.AddGraphPattern(this._rhs.ToGraphPattern());
            return p;
        }

        /// <summary>
        /// Transforms both sides of the Join using the given Optimiser
        /// </summary>
        /// <param name="optimiser">Optimser</param>
        /// <returns></returns>
        public ISparqlAlgebra Transform(IAlgebraOptimiser optimiser)
        {
            return new Join(optimiser.Optimise(this._lhs), optimiser.Optimise(this._rhs));
        }

        /// <summary>
        /// Transforms the LHS of the Join using the given Optimiser
        /// </summary>
        /// <param name="optimiser">Optimser</param>
        /// <returns></returns>
        public ISparqlAlgebra TransformLhs(IAlgebraOptimiser optimiser)
        {
            return new Join(optimiser.Optimise(this._lhs), this._rhs);
        }

        /// <summary>
        /// Transforms the RHS of the Join using the given Optimiser
        /// </summary>
        /// <param name="optimiser">Optimser</param>
        /// <returns></returns>
        public ISparqlAlgebra TransformRhs(IAlgebraOptimiser optimiser)
        {
            return new Join(this._lhs, optimiser.Optimise(this._rhs));
        }
    }
}
