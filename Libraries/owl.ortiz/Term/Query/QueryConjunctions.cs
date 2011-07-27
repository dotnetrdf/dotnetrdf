using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.OWL.Term.Query
{
    public interface IQueryAnd
        : IAnd<IQueryAtom>, ICompositeQueryAtom
    {

    }

    public interface IQueryOr
        : IOr<IQueryAtom>, ICompositeQueryAtom
    {

    }

    public interface IQueryNot
        : INot<IQueryAtom>, ICompositeQueryAtom
    {

    }
}
