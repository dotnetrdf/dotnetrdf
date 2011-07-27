using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.OWL.Term.Entity
{
    public interface IDataType
        : IDataTypeExpression
    {
        IDataAnd CreateAnd(IEnumerable<IDataType> types);

        IDataAnd CreateAnd(IDataType type);

        IDataOr CreateOr(IEnumerable<IDataType> types);

        IDataOr CreateOr(IDataType type);
    }

    public interface INAryDataType
        : IDataTypeExpression
    {
        int Arity
        {
            get;
        }
    }
}
