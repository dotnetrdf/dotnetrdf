using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.OWL.Term.Axiom;

namespace VDS.OWL.Term.Entity
{
    public interface IEntity 
        : ITerm
    {
        bool IsVariable
        {
            get;
        }
    }
}
