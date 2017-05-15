using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.OWL.Term.Function
{
    public interface IFunction
        : INamedTerm
    {
        int Arity
        {
            get;
        }

        int MaxArity
        {
            get;
        }

        int MinArity
        {
            get;
        }

        bool IsFixedArity
        {
            get;
        }
    }
}
