using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.OWL.Term.Entity;
using VDS.OWL.Term.Query;

namespace VDS.OWL.Term.Axiom
{
    public interface IDeclaration
        : IAxiom, IAtomicQueryAtom
    {
        INamedEntity Entity
        {
            get;
        }
    }
}
