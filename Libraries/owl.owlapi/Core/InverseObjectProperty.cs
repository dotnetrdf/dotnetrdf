using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.OWL
{
    public class InverseObjectProperty : ObjectPropertyExpression
    {
        public ObjectProperty Property { get; protected set; }
    }
}
