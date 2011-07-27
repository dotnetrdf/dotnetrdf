using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.OWL.Term.Entity
{
    public interface IObjectAnd
        : IAnd<IClassExpression>, IClassExpression
    {

    }

    public interface IObjectNot
        : INot<IClassExpression>, IClassExpression
    {

    }

    public interface IObjectOneOf
        : IOneOf<IIndividualExpression>, IClassExpression
    {

    }

    public interface IObjectOr
        : IOr<IClassExpression>, IClassExpression
    {

    }
}
