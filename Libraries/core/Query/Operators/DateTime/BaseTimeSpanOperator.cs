using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;

namespace VDS.RDF.Query.Operators.DateTime
{
    public abstract class BaseTimeSpanOperator
        : ISparqlOperator
    {

        public abstract SparqlOperatorType Operator
        {
            get;
        }

        public bool IsApplicable(params IValuedNode[] ns)
        {
            return !Options.StrictOperators
                   && ns.Length > 0
                   && ns.All(n => n != null && (n.EffectiveType.Equals(XmlSpecsHelper.XmlSchemaDataTypeDayTimeDuration) || n.EffectiveType.Equals(XmlSpecsHelper.XmlSchemaDataTypeDuration)));
        }

        public abstract IValuedNode Apply(params IValuedNode[] ns);
    }
}
