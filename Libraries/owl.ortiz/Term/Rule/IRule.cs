using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.OWL.Term.Axiom;

namespace VDS.OWL.Term.Rule
{
    public interface IRule
        : IAxiom
    {
        ITermSet<IRuleAtom> Body
        {
            get;
        }

        ITermSet<IRuleAtom> Head
        {
            get;
        }
    }
}
