using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.OWL.Term.Query;
using VDS.OWL.Term.Rule;

namespace VDS.OWL.Term.Axiom
{
    public interface IAssertion
        : IAxiom, IRuleAtom, IAtomicQueryAtom
    {
    }
}
