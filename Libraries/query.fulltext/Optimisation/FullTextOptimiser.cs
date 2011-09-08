using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Update;
using VDS.RDF.Query.FullText.Search;

namespace VDS.RDF.Query.Optimisation
{
    public class FullTextOptimiser
        : IAlgebraOptimiser
    {
        private IFullTextSearchProvider _provider;

        public FullTextOptimiser(IFullTextSearchProvider provider)
        {
            if (provider == null) throw new ArgumentNullException("Full Text Search Provider cannot be null");
            this._provider = provider;
        }

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
                        if (!(ps[i] is TriplePattern))
                        {
                            patterns.Add(ps[i]);
                        }
                        else
                        {
                            //Check to see if the Predicate of the Pattern match the Full Text Match Predicate URI
                            TriplePattern tp = (TriplePattern)ps[i];
                            PatternItem pred = tp.Predicate;
                            if (pred is NodeMatchPattern)
                            {
                                INode predNode = ((NodeMatchPattern)pred).Node;
                                if (predNode.NodeType == NodeType.Uri)
                                {
                                    String predUri = ((IUriNode)predNode).Uri.ToString();
                                    if (predUri.Equals(FullTextHelper.FullTextMatchPredicateUri))
                                    {
                                        if (patterns.Count > 0) result = Join.CreateJoin(result, new Bgp(patterns));
                                        result = new FullTextMatch(this._provider, result, tp.Subject, tp.Object);
                                        patterns.Clear();
                                    }
                                    else
                                    {
                                        patterns.Add(ps[i]);
                                    }
                                }
                                else
                                {
                                    patterns.Add(ps[i]);
                                }
                            }
                            else
                            {
                                patterns.Add(ps[i]);
                            }
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

        public bool IsApplicable(SparqlUpdateCommandSet cmds)
        {
            return true;
        }
    }
}
