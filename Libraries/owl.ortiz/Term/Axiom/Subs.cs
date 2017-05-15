using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.OWL.Term.Entity;

namespace VDS.OWL.Term.Axiom
{
    public interface ISubPropertyOf<TSub, TSuper>
        : ISubsumption<TSub, TSuper>
        where TSub : IPropertyExpression
        where TSuper : IProperty
    {

    }

    public interface ISubAnnotationPropertyOf
        : ISubPropertyOf<IAnnotationProperty, IAnnotationProperty>, IAnnotationPropertyAxiom
    {

    }

    public interface ISubClassOf
        : ISubsumption<IClassExpression, IClassExpression>, IClassAxiom
    {

    }

    public interface ISubDataPropertyOf
        : ISubPropertyOf<IDataProperty, IDataProperty>, IPropertyAxiom
    {

    }

    public interface ISubObjectPropertyChain
        : ISubPropertyOf<IObjectPropertyList, IObjectProperty>, IPropertyAxiom
    {

    }

    public interface ISubObjectPropertyOf
        : ISubPropertyOf<IObjectProperty, IObjectProperty>, IPropertyAxiom
    {

    }
}
