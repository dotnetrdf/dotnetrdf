using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.OWL.Term.Entity;
using VDS.OWL.Term.Query;
using VDS.OWL.Term.Rule;

namespace VDS.OWL.Term.Function
{
    public interface IFunctionAtom
        : ITermList<IEntity>, IRuleAtom, IQueryAtom
    {
        IFunction Function
        {
            get;
        }
    }
}
