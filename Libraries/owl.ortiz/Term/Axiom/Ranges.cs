using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.OWL.Term.Entity;

namespace VDS.OWL.Term.Axiom
{
    public interface IAnnotationPropertyRange
        : IPropertyRange<IAnnotationProperty, IType>, IAnnotationPropertyAxiom
    {

    }

    public interface IPropertyRange<TProperty, TType>
        where TProperty : IProperty
        where TType : IType
    {
        TProperty Property
        {
            get;
        }

        TType Range
        {
            get;
        }
    }

    public interface IDataPropertyRange
        : IPropertyRange<IDataProperty, IDataType>, IPropertyAxiom
    {

    }

    public interface IObjectPropertyRange
        : IPropertyRange<IObjectProperty, IClassExpression>, IPropertyAxiom
    {

    }
}
