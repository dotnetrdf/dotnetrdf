using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Optimisation
{
    /// <summary>
    /// The Strict Algebra Optimiser is an optimiser that takes our BGPs which typically contain placed FILTERs and BINDs and transforms them into their strict algebra form using Filter() and Extend()
    /// </summary>
    public class StrictAlgebraOptimiser : IAlgebraOptimiser
    {
        /// <summary>
        /// Optimises BGPs in the Algebra to use Filter() and Extend() rather than the embedded FILTER and BIND
        /// </summary>
        /// <param name="algebra">Algebra to optimise</param>
        /// <returns></returns>
        public ISparqlAlgebra Optimise(ISparqlAlgebra algebra)
        {
            if (algebra is IAbstractJoin)
            {
                return ((IAbstractJoin)algebra).Transform(this);
            }
            else if (algebra is IUnaryOperator)
            {
                return ((IUnaryOperator)algebra).Transform(this);
            }
            else if (algebra is IBgp)
            {
                IBgp current = (IBgp)algebra;
                if (current.PatternCount == 0)
                {
                    return current;
                }
                else
                {
                    ISparqlAlgebra result = new Bgp();
                    List<ITriplePattern> patterns = new List<ITriplePattern>();
                    List<ITriplePattern> ps = new List<ITriplePattern>(current.TriplePatterns.ToList());
                    for (int i = 0; i < current.PatternCount; i++)
                    {
                        if (ps[i] is FilterPattern || ps[i] is BindPattern)
                        {
                            //First ensure that if we've found any other Triple Patterns up to this point
                            //we dump this into a BGP and join with the result so far
                            if (patterns.Count > 0)
                            {
                                result = Join.CreateJoin(result, new Bgp(patterns));
                                patterns.Clear();
                            }
                            if (ps[i] is FilterPattern)
                            {
                                result = new Filter(result, ((FilterPattern)ps[i]).Filter);
                            }
                            else
                            {
                                BindPattern bind = (BindPattern)ps[i];
                                result = new Extend(result, bind.AssignExpression, bind.VariableName);
                            }
                        }
                        else
                        {
                            patterns.Add(ps[i]);
                        }
                    }

                    if (patterns.Count == current.PatternCount)
                    {
                        //If count of remaining patterns same as original pattern count there was no optimisation
                        //to do so return as is
                        return current;
                    }
                    else if (patterns.Count > 0)
                    {
                        //If any patterns left at end join as a BGP with result so far
                        result = Join.CreateJoin(result, new Bgp(patterns));
                        return result;
                    }
                    else
                    {
                        return result;
                    }
                }
            }
            else if (algebra is ITerminalOperator)
            {
                return algebra;
            }
            else
            {
                return algebra;
            }
        }

        public bool IsApplicable(SparqlQuery q)
        {
            return true;
        }
    }
}
