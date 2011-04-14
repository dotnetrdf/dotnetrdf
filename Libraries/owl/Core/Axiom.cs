using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.OWL
{
    public abstract class BaseAxiom : IAxiom
    {
        public IEnumerable<IAnnotation> Annotations { get; protected set; }
    }

    public abstract class Axiom : BaseAxiom
    {
    }

}
