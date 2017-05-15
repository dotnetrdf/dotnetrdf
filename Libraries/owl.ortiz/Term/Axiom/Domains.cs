using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.OWL.Term.Entity;

namespace VDS.OWL.Term.Axiom
{
    public interface IAnnotationPropertyDomain
        : IPropertyDomain<IAnnotationProperty>, IAnnotationPropertyAxiom
    {

    }

    public interface IPropertyDomain<T>
        where T : IProperty
    {
        T Property
        {
            get;
        }

        IClassExpression Domain
        {
            get;
        }
    }

    public interface IDataPropertyDomain
        : IPropertyDomain<IDataProperty>, IPropertyAxiom
    {

    }

    public interface IObjectPropertyDomain
        : IPropertyDomain<IObjectProperty>, IPropertyAxiom
    {

    }

}
