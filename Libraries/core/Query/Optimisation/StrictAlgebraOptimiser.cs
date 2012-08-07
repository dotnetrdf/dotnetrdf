/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

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

using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Update;

namespace VDS.RDF.Query.Optimisation
{
    /// <summary>
    /// The Strict Algebra Optimiser is an optimiser that takes our BGPs which typically contain placed FILTERs and BINDs and transforms them into their strict algebra form using Filter() and Extend()
    /// </summary>
    public class StrictAlgebraOptimiser 
        : IAlgebraOptimiser
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
                //Don't integerfer with other optimisers which have added custom BGP implementations
                if (!(algebra is Bgp)) return algebra;

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
                        //Can't split the BGP if there are Blank Nodes present
                        if (!ps[i].HasNoBlankVariables) return current;

                        if (ps[i].PatternType != TriplePatternType.Match)
                        {
                            //First ensure that if we've found any other Triple Patterns up to this point
                            //we dump this into a BGP and join with the result so far
                            if (patterns.Count > 0)
                            {
                                result = Join.CreateJoin(result, new Bgp(patterns));
                                patterns.Clear();
                            }

                            //Then generate the appropriate strict algebra operator
                            switch (ps[i].PatternType)
                            {
                                case TriplePatternType.Filter:
                                    result = new Filter(result, ((IFilterPattern)ps[i]).Filter);
                                    break;
                                case TriplePatternType.BindAssignment:
                                case TriplePatternType.LetAssignment:
                                    IAssignmentPattern assignment = (IAssignmentPattern)ps[i];
                                    result = new Extend(result, assignment.AssignExpression, assignment.VariableName);
                                    break;
                                case TriplePatternType.SubQuery:
                                    ISubQueryPattern sq = (ISubQueryPattern)ps[i];
                                    result = Join.CreateJoin(result, new SubQuery(sq.SubQuery));
                                    break;
                                case TriplePatternType.Path:
                                    IPropertyPathPattern pp = (IPropertyPathPattern)ps[i];
                                    result = Join.CreateJoin(result, new PropertyPath(pp.Subject, pp.Path, pp.Object));
                                    break;
                                default:
                                    throw new RdfQueryException("Cannot apply strict algebra form to a BGP containing a unknown triple pattern type");
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

        /// <summary>
        /// Returns that the optimiser is applicable to all queries
        /// </summary>
        /// <param name="q">Query</param>
        /// <returns></returns>
        public bool IsApplicable(SparqlQuery q)
        {
            return true;
        }

        /// <summary>
        /// Returns that the optimiser is applicable to all updates
        /// </summary>
        /// <param name="cmds">Updates</param>
        /// <returns></returns>
        public bool IsApplicable(SparqlUpdateCommandSet cmds)
        {
            return true;
        }
    }
}
