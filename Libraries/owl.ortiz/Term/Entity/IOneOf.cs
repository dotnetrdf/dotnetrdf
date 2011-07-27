using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.OWL.Term.Entity
{
    public interface IOneOf<T>
        : ITermCollection<T>
        where T : IValue
    {
    }
}
