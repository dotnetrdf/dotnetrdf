using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.OWL
{
    public abstract class DataPropertyRestriction : ClassExpression
    {
    }

    public abstract class DataPropertyValueRangeRestriction : DataPropertyRestriction
    {
        public IDataRange DataRange { get; protected set; }

        public IOrderedEnumerable<DataPropertyExpression> PropertyExpressions { get; protected set; }
    }

    public class DataSomeValuesFrom : DataPropertyValueRangeRestriction
    {

    }

    public class DataAllValuesFrom : DataPropertyValueRangeRestriction
    {

    }

    public class DataHasValue : DataPropertyRestriction
    {
        public DataPropertyExpression PropertyExpression { get; protected set; }

        public ILiteral Value { get; protected set; }
    }

    public abstract class DataPropertyCardinalityRestriction : DataPropertyRestriction
    {
        public long Cardinality { get; protected set; }

        public IDataRange Range { get; protected set; }
    }

    public class DataMinCardinality : DataPropertyCardinalityRestriction
    {

    }

    public class DataMaxCardinality : DataPropertyCardinalityRestriction
    {

    }

    public class DataExactCardinality : DataPropertyCardinalityRestriction
    {

    }
}
