using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.OWL.Term
{
    public interface INamedTerm : ITerm
    {
        String Name
        {
            get;
        }
    }
}
