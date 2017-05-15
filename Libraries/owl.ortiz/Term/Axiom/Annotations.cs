using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.OWL.Term.Entity;

namespace VDS.OWL.Term.Axiom
{
    public interface IAxiomAnnotation
        : IAnnotation<IAxiom>
    {

    }

    public interface IEntityAnnotation
        : IAnnotation<INamedEntity>
    {

    }

    public interface IOntologyAnnotation
        : IAnnotation<INamedTerm>
    {

    }
}
