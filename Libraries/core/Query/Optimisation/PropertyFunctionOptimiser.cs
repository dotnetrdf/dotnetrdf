using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Query.PropertyFunctions;
using VDS.RDF.Update;

namespace VDS.RDF.Query.Optimisation
{
    public class PropertyFunctionOptimiser
        : IAlgebraOptimiser
    {
        public ISparqlAlgebra Optimise(ISparqlAlgebra algebra)
        {
            if (algebra is IBgp)
            {
                IBgp current = (IBgp)algebra;
                if (current.PatternCount == 0) return current;

                List<ITriplePattern> ps = current.TriplePatterns.ToList();
                List<IPropertyFunctionPattern> propFuncs = PropertyFunctionHelper.ExtractPatterns(ps);
                if (propFuncs.Count == 0) return current;

                //Remove raw Triple Patterns pertaining to extracted property functions
                foreach (IPropertyFunctionPattern propFunc in propFuncs)
                {
                    //Track where we need to insert the property function back into the BGP
                    ITriplePattern first = propFunc.OriginalPatterns.First();
                    int origLocation = ps.FindIndex(p => ps.Equals(first));
                    foreach (ITriplePattern tp in propFunc.OriginalPatterns)
                    {
                        int location = ps.FindIndex(p => p.Equals(tp));
                        if (location >= 0)
                        {
                            if (location < origLocation) origLocation--;
                            ps.RemoveAt(location);
                        }
                        else
                        {
                            throw new RdfQueryException("Bad Property Function extraction");
                        }
                    }

                    //Make the insert
                    if (origLocation >= ps.Count)
                    {
                        ps.Add(propFunc);
                    }
                    else
                    {
                        ps.Insert(origLocation, propFunc);
                    }
                }

                //Return a new BGP
                return new Bgp(ps);
            }
            else if (algebra is ITerminalOperator)
            {
                return algebra;
            }
            else if (algebra is IAbstractJoin)
            {
                return ((IAbstractJoin)algebra).Transform(this);
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
