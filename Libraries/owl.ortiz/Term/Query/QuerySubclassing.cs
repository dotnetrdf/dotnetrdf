using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.OWL.Term.Entity;

namespace VDS.OWL.Term.Query
{
    public interface IDirectSubClassOf
        : ISubsumption<IClassExpression, IClassExpression>, IAtomicQueryAtom
    {

    }

    public interface IDirectSubDataPropertyOf
        : ISubsumption<IDataProperty, IDataProperty>, IAtomicQueryAtom
    {

    }

    public interface IDirectSubObjectPropertyOf
        : ISubsumption<IObjectProperty, IObjectProperty>, IAtomicQueryAtom
    {

    }

    public interface IDirectTypeAssertion
        : IAtomicQueryAtom
    {
        IClassExpression Type
        {
            get;
        }

        IIndividualExpression Individual
        {
            get;
        }
    }

    public interface IStrictSubClassOf
        : ISubsumption<IClassExpression, IClassExpression>, IAtomicQueryAtom
    {

    }

    public interface IStrictSubDataPropertyOf
        : ISubsumption<IDataProperty, IDataProperty>, IAtomicQueryAtom
    {

    }

    public interface IStrictSubObjectPropertyOf
        : ISubsumption<IObjectProperty, IObjectProperty>, IAtomicQueryAtom
    {

    }
}
