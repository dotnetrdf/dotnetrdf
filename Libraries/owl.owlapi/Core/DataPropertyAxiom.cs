using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.OWL
{
    public abstract class DataPropertyAxiom : Axiom
    {
    }

    public abstract class UnaryDataPropertyAxiom : DataPropertyAxiom
    {
        public DataPropertyExpression Property { get; protected set; }
    }

    public class SubDataPropertyOfAxiom : DataPropertyAxiom
    {
        public DataPropertyExpression SubProperty { get; protected set; }

        public DataPropertyExpression SuperProperty { get; protected set; }
    }

    public class DisjointDataPropertiesAxiom : DataPropertyAxiom
    {
        public IEnumerable<DataPropertyExpression> DisjointProperties { get; protected set; }
    }

    public class EquivalentDataPropertiesAxiom : DataPropertyAxiom
    {
        public IEnumerable<DataPropertyExpression> EquivalentProperties { get; protected set; }
    }

    public class FunctionalDataPropertyAxiom : UnaryDataPropertyAxiom
    {
    }

    public class DataPropertyDomainAxiom : UnaryDataPropertyAxiom
    {
        public ClassExpression Domain { get; protected set; }
    }

    public class DataPropertyRangeAxiom : UnaryDataPropertyAxiom
    {
        public IDataRange Range { get; protected set; }
    }
}
