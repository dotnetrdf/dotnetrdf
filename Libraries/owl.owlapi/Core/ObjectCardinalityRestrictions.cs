using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.OWL
{
    public abstract class ObjectCardinalityRestriction
    {
        public ClassExpression ClassExpression { get; protected set; }

        public long Cardinality { get; protected set; }

        public ObjectPropertyExpression PropertyExpression { get; protected set; }
    }

    public class ObjectMaxCardinality : ObjectCardinalityRestriction
    {

    }

    public class ObjectMinCardinality : ObjectCardinalityRestriction
    {

    }

    public class ObjectExactCardinality : ObjectCardinalityRestriction
    {

    }
}
