using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.OWL
{
    public class DatatypeDefinitionAxiom : Axiom
    {
        public IDataRange DataRange { get; protected set; }

        public IDataType DataType { get; protected set; }
    }
}
