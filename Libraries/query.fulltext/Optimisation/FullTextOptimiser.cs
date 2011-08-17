using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Update;

namespace VDS.RDF.Query.Optimisation
{
    public class FullTextOptimiser
        : IAlgebraOptimiser
    {
        private const String TextMatchFunctionUri = "http://jena.hpl.hp.com/ARQ/property#textMatch";

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
                //TODO: Rewrite this so it extracts Full Text Search into the custom operator
                return algebra;

                //IBgp current = (IBgp)algebra;
                //if (current.PatternCount == 0)
                //{
                //    return current;
                //}
                //else
                //{
                //    ISparqlAlgebra result = new Bgp();
                //    List<ITriplePattern> patterns = new List<ITriplePattern>();
                //    List<ITriplePattern> ps = new List<ITriplePattern>(current.TriplePatterns.ToList());
                //    for (int i = 0; i < current.PatternCount; i++)
                //    {
                //        if (!(ps[i] is TriplePattern))
                //        {
                //            //First ensure that if we've found any other Triple Patterns up to this point
                //            //we dump this into a BGP and join with the result so far
                //            if (patterns.Count > 0)
                //            {
                //                result = Join.CreateJoin(result, new Bgp(patterns));
                //                patterns.Clear();
                //            }

                //            //Then generate the appropriate strict algebra operator
                //            if (ps[i] is FilterPattern)
                //            {
                //                result = new Filter(result, ((FilterPattern)ps[i]).Filter);
                //            }
                //            else if (ps[i] is BindPattern)
                //            {
                //                BindPattern bind = (BindPattern)ps[i];
                //                result = new Extend(result, bind.AssignExpression, bind.VariableName);
                //            }
                //            else if (ps[i] is LetPattern)
                //            {
                //                LetPattern let = (LetPattern)ps[i];
                //                result = new Extend(result, let.AssignExpression, let.VariableName);
                //            }
                //            else if (ps[i] is SubQueryPattern)
                //            {
                //                SubQueryPattern sq = (SubQueryPattern)ps[i];
                //                result = Join.CreateJoin(result, new SubQuery(sq.SubQuery));
                //            }
                //            else if (ps[i] is PropertyPathPattern)
                //            {
                //                PropertyPathPattern pp = (PropertyPathPattern)ps[i];
                //                result = Join.CreateJoin(result, new PropertyPath(pp.Subject, pp.Path, pp.Object));
                //            }
                //        }
                //        else
                //        {
                //            patterns.Add(ps[i]);
                //        }
                //    }

                //    if (patterns.Count == current.PatternCount)
                //    {
                //        //If count of remaining patterns same as original pattern count there was no optimisation
                //        //to do so return as is
                //        return current;
                //    }
                //    else if (patterns.Count > 0)
                //    {
                //        //If any patterns left at end join as a BGP with result so far
                //        result = Join.CreateJoin(result, new Bgp(patterns));
                //        return result;
                //    }
                //    else
                //    {
                //        return result;
                //    }
                //}
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
