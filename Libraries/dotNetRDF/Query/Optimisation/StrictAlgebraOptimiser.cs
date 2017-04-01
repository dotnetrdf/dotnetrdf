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
                // Don't integerfer with other optimisers which have added custom BGP implementations
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
                        // Can't split the BGP if there are Blank Nodes present
                        if (!ps[i].HasNoBlankVariables) return current;

                        if (ps[i].PatternType != TriplePatternType.Match)
                        {
                            // First ensure that if we've found any other Triple Patterns up to this point
                            // we dump this into a BGP and join with the result so far
                            if (patterns.Count > 0)
                            {
                                result = Join.CreateJoin(result, new Bgp(patterns));
                                patterns.Clear();
                            }

                            // Then generate the appropriate strict algebra operator
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
                                case TriplePatternType.PropertyFunction:
                                    IPropertyFunctionPattern pf = (IPropertyFunctionPattern)ps[i];
                                    result = new PropertyFunction(result, pf.PropertyFunction);
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
                        // If count of remaining patterns same as original pattern count there was no optimisation
                        // to do so return as is
                        return current;
                    }
                    else if (patterns.Count > 0)
                    {
                        // If any patterns left at end join as a BGP with result so far
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
