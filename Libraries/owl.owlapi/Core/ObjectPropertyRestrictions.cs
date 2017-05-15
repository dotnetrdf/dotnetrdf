using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.OWL
{
    public abstract class ObjectPropertyRestriction : ClassExpression
    {
        public ObjectPropertyExpression PropertyExpression { get; protected set; }
    }

    public class ObjectAllValuesFrom : ObjectPropertyRestriction
    {
        public ClassExpression ClassExpression { get; protected set; }
    }

    public class ObjectSomeValuesFrom : ObjectPropertyRestriction
    {
        public ClassExpression ClassExpression { get; protected set; }
    }

    public class ObjectHasSelf : ClassExpression
    {

    }

    public class ObjectHasValue : ClassExpression
    {
        public IIndividual Individual { get; protected set; }
    }


}
