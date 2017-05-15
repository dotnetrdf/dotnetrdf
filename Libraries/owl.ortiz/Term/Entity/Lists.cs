using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.OWL.Term.Axiom;

namespace VDS.OWL.Term.Entity
{
    public interface IPropertyList<T>
        : ITermList<T>, IPropertyExpression
        where T : IProperty
    {

    }

    public interface IDataPropertyList
        : IPropertyList<IDataProperty>
    {

    }

    public interface IObjectPropertyList
        : IPropertyList<IObjectProperty>
    {
        IObjectPropertyList CreateInverse();

        ISubObjectPropertyChain CreateSubPropertyOf(IObjectProperty p);
    }
}
