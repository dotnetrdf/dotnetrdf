using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.OWL.Term.Entity
{
    public interface IFacet
        : INamedTerm
    {
    }

    public interface IFacetRestriction
        : ITerm
    {
        IFacet Facet
        {
            get;
        }

        ILiteral Value
        {
            get;
        }
    }
}
