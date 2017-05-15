using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.OWL.Term.Entity;

namespace VDS.OWL.Term.Axiom
{
    public interface IDisjointProperties<T>
        : ITermSet<T>, IPropertyAxiom
        where T : IProperty
    {

    }

    public interface IEquivalentProperties<T>
        : ITermSet<T>, IPropertyAxiom
        where T : IProperty
    {

    }
}
