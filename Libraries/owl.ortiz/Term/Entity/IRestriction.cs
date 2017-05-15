using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.OWL.Term.Axiom;

namespace VDS.OWL.Term.Entity
{
    public interface IRestriction<T>
        : IClassExpression
        where T : IPropertyExpression
    {
        T Property
        {
            get;
        }
    }

    public interface IQualifiedRestriction<TProperty, TType>
        : IRestriction<TProperty>
        where TProperty : IPropertyExpression
        where TType : IType
    {
        TType Qualification
        {
            get;
        }
    }

    public interface IQualifiedCardinalityRestriction<TProperty, TType>
        : IQualifiedRestriction<TProperty, TType>
        where TProperty : IProperty
        where TType : IType
    {
        int Cardinality
        {
            get;
        }
    }

    public interface IExactCardinalityRestriction<TProperty, TType>
        : IQualifiedCardinalityRestriction<TProperty, TType>
        where TProperty : IProperty
        where TType : IType
    {

    }

    public interface IMaxCardinalityRestriction<TProperty, TType>
        : IQualifiedCardinalityRestriction<TProperty, TType>
        where TProperty : IProperty
        where TType : IType
    {

    }

    public interface IMinCardinalityRestriction<TProperty, TType>
        : IQualifiedCardinalityRestriction<TProperty, TType>
        where TProperty : IProperty
        where TType : IType
    {

    }

    public interface ISomeRestriction<TProperty, TType>
        : IQualifiedRestriction<TProperty, TType>
        where TProperty : IProperty
        where TType : IType
    {

    }

    public interface IDataRestriction
        : IRestriction<IDataProperty>
    {

    }

    public interface IDataQualifiedRestriction
        : IDataRestriction, IQualifiedRestriction<IDataProperty, IDataType>
    {

    }

    public interface IDataQualifiedCardinalityRestriction
        : IDataQualifiedRestriction, IQualifiedCardinalityRestriction<IDataProperty, IDataType>
    {

    }

    public interface IObjectRestriction
    : IRestriction<IObjectProperty>
    {

    }

    public interface IObjectQualifiedRestriction
        : IObjectRestriction, IQualifiedRestriction<IObjectProperty, IClassExpression>
    {

    }

    public interface IObjectQualifiedCardinalityRestriction
        : IObjectQualifiedRestriction, IQualifiedCardinalityRestriction<IObjectProperty, IClassExpression>
    {

    }
}
