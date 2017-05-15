using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.OWL.Term.Entity;
using VDS.OWL.Term.Query;

namespace VDS.OWL.Term.Axiom
{
    public interface IClassAxiom
        : IAxiom, IAtomicQueryAtom
    {

    }

    public interface IAnnotationPropertyAxiom
        : IAxiom, IAtomicQueryAtom
    {

    }

    public interface IPropertyAxiom
        : IAxiom, IQueryAtom
    {

    }

    public interface IUnaryPropertyAxiom<T>
        : IProperty
        where T : IProperty
    {

    }

    public interface IDifferentIndividuals
        : ITermSet<IIndividualExpression>, IAssertion
    {

    }

    public interface IDisjointClasses
        : ITermSet<IClassExpression>, IClassAxiom
    {

    }

    public interface IDisjointDataProperties
        : IDisjointProperties<IDataProperty>
    {

    }

    public interface IDisjointObjectProperties
        : IDisjointProperties<IObjectProperty>
    {

    }

    public interface IDisjointUnion
        : IClassAxiom
    {
        ITermSet<IClassExpression> Definition
        {
            get;
        }

        INamedClass NamedClass
        {
            get;
        }
    }

    public interface IEquivalentClasses
        : ITermSet<IClassExpression>, IClassAxiom
    {

    }

    public interface IEquivalentDataProperties
        : IEquivalentProperties<IDataProperty>
    {

    }

    public interface IEquivalentObjectProperties
        : IEquivalentProperties<IObjectProperty>
    {

    }

    public interface IHasKey
        : IAxiom
    {
        ITermSet<IDataProperty> DataProperties
        {
            get;
        }

        IClassExpression KeyClass
        {
            get;
        }

        ITermSet<IObjectProperty> ObjectProperties
        {
            get;
        }

        ITermSet<IProperty> Properties
        {
            get;
        }
    }

    public interface IInverseFunctional
        : IUnaryPropertyAxiom<IObjectProperty>
    {

    }

    public interface IInverseProperties
        : IPropertyAxiom
    {
        IObjectProperty InverseProperty
        {
            get;
        }

        IObjectProperty Property
        {
            get;
        }
    }
}
