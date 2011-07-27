using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.OWL
{
    public class DataProperty : DataPropertyExpression, IEntity
    {
        public Iri EntityIri { get; protected set; }
    }
}
