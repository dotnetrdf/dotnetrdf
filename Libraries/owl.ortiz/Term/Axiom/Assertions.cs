using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.OWL.Term.Entity;

namespace VDS.OWL.Term.Axiom
{
    public interface IPropertyAssertion<TProperty, TValue>
        : IAssertion
        where TProperty : IProperty
        where TValue : IValue
    {
        TValue Object
        {
            get;
        }

        TProperty Property
        {
            get;
        }

        IIndividualExpression Subject
        {
            get;
        }
    }

    public interface IPositivePropertyAssertion<TProperty, TValue>
        : IPropertyAssertion<TProperty, TValue>
        where TProperty : IProperty
        where TValue : IValue
    {

    }

    public interface INegativePropertyAssertion<TProperty, TValue>
        : IPropertyAssertion<TProperty, TValue>
        where TProperty : IProperty
        where TValue : IValue
    {

    }

    public interface IDataPropertyAssertion
        : IPositivePropertyAssertion<IDataProperty, ILiteralExpression>
    {

    }

    public interface INegativeDataPropertyAssertion
        : INegativePropertyAssertion<IDataProperty, ILiteralExpression>
    {

    }

    public interface IObjectPropertyAssertion
        : IPositivePropertyAssertion<IObjectProperty, ILiteralExpression>
    {

    }

    public interface INegativeObjectPropertyAssertion
        : INegativePropertyAssertion<IObjectProperty, ILiteralExpression>
    {

    }

    public interface ISameIndividual
        : ITermSet<IIndividualExpression>, IAssertion
    {

    }

    public interface ITypeAssertion
        : IAssertion
    {
        IIndividualExpression Individual
        {
            get;
        }

        IClassExpression Type
        {
            get;
        }
    }
}
