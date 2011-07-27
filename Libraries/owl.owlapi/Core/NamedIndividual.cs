using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.OWL
{
    public class NamedIndividual : Individual, IEntity
    {
        public Iri EntityIri { get; protected set; }
    }
}
