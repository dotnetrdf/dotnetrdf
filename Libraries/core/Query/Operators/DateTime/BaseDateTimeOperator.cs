using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace VDS.RDF.Query.Operators.DateTime
{
    public abstract class BaseDateTimeOperator
        : ISparqlOperator
    {
        public abstract SparqlOperatorType Operator
        {
            get;
        }

        public bool IsApplicable(params IValuedNode[] ns)
        {
            return !Options.StrictOperators 
                   && ns.Length == 2
                   && ns.All(n => n!= null) 
                   && ns[0].EffectiveType.Equals(XmlSpecsHelper.XmlSchemaDataTypeDateTime) 
                   && (ns[1].EffectiveType.Equals(XmlSpecsHelper.XmlSchemaDataTypeDayTimeDuration) || ns[1].EffectiveType.Equals(XmlSpecsHelper.XmlSchemaDataTypeDuration));
        }

        public abstract IValuedNode Apply(params IValuedNode[] ns);
    }
}
