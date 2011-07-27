using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.OWL
{
    public class FacetRestriction
    {
        public ILiteral Value { get; protected set; }

        public Iri Constraint { get; protected set; }
    }
}
