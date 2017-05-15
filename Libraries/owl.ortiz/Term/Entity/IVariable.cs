using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.OWL.Term.Entity
{
    public interface IVariable
        : INamedTerm, IEntity
    {
        bool IsVariable
        {
            get;
        }
    }
}
