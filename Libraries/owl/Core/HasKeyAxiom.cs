using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.OWL
{
    public class HasKeyAxiom : Axiom
    {
        public IEnumerable<ObjectPropertyExpression> ObjectProperties { get; protected set; }

        public IEnumerable<DataPropertyExpression> DataProperties { get; protected set; }

        public ClassExpression Class { get; protected set; }
    }
}
