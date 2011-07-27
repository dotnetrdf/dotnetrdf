using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.OWL.Term.Entity
{
    public interface ILiteralExpression
        : IValue
    {
    }

    public interface ILiteralVariable
        : INamedTerm, ILiteralExpression, IVariable
    {

    }
}
