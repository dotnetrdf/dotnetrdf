using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.OWL.Term.Axiom;

namespace VDS.OWL.Term.Entity
{
    public interface IAllRestriction<TProperty, TType>
        : IQualifiedRestriction<TProperty, TType>
        where TProperty : IProperty
        where TType : IType
    {

    }

    public interface IHasValue<TProperty, TValue>
    : IRestriction<TProperty>
        where TProperty : IPropertyExpression
        where TValue : IValue
    {
        TValue Value
        {
            get;
        }
    }

    public interface IDataAll
        : IDataQualifiedRestriction, IAllRestriction<IDataProperty, IType>
    {

    }

    public interface IDataCardinality
        : IDataQualifiedCardinalityRestriction, IExactCardinalityRestriction<IDataProperty, IDataType>
    {

    }

    public interface IDataHasValue
        : IDataRestriction, IHasValue<IDataProperty, ILiteralExpression>
    {

    }

    public interface IDataMax
        : IDataQualifiedCardinalityRestriction, IMaxCardinalityRestriction<IDataProperty, IDataType>
    {

    }

    public interface IDataMin
        : IDataQualifiedCardinalityRestriction, IMinCardinalityRestriction<IDataProperty, IDataType>
    {

    }

    public interface IDataNAryAll
        : IQualifiedRestriction<IDataPropertyList, INAryDataType>
    {

    }

    public interface IDataNArySome
        : IQualifiedRestriction<IDataPropertyList, INAryDataType>
    {

    }

    public interface IDataSome
        : IDataQualifiedRestriction, ISomeRestriction<IDataProperty, IDataType>
    {

    }

    public interface IObjectAll
        : IObjectQualifiedRestriction, IAllRestriction<IObjectProperty, IClassExpression>
    {

    }

    public interface IObjectCardinality
        : IObjectQualifiedCardinalityRestriction, IExactCardinalityRestriction<IObjectProperty, IClassExpression>
    {

    }

    public interface IObjectHasValue
        : IObjectRestriction, IHasValue<IObjectProperty, IIndividualExpression>
    {

    }

    public interface IObjectMax
        : IObjectQualifiedCardinalityRestriction, IMaxCardinalityRestriction<IObjectProperty, IClassExpression>
    {

    }

    public interface IObjectMin
        : IObjectQualifiedCardinalityRestriction, IMinCardinalityRestriction<IObjectProperty, IClassExpression>
    {

    }

    public interface IObjectSelf
        : IObjectRestriction
    {

    }

    public interface IObjectSome
        : IObjectQualifiedRestriction, ISomeRestriction<IObjectProperty, IClassExpression>
    {

    }
}
