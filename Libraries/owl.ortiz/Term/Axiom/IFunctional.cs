using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.OWL.Term.Entity;

namespace VDS.OWL.Term.Axiom
{
    public interface IFunctional<T>
        : IUnaryPropertyAxiom<T>
        where T : IProperty
    {

    }
}
