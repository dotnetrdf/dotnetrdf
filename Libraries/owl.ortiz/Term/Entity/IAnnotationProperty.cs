using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.OWL.Term.Axiom;

namespace VDS.OWL.Term.Entity
{
    public interface IAnnotationProperty
        : IProperty
    {
        IAnnotationPropertyDomain CreateDomain(IClassExpression domain);

        IAnnotationPropertyRange CreateRange(IType range);

        ISubAnnotationPropertyOf CreateSubPropertyOf(IAnnotationProperty property);
    }

    public interface IAnnotationPropertyVariable
        : INamedTerm, IAnnotationProperty, IVariable
    {

    }

}
