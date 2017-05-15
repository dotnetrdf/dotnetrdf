using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.OWL.Term.Query
{
    public interface IQueryOptional : ITermSet<IQueryAtom>, ICompositeQueryAtom
    {
    }
}
