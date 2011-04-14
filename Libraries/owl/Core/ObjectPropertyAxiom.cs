using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.OWL
{
    public abstract class ObjectPropertyAxiom : Axiom
    {
    }

    public abstract class UnaryObjectPropertyAxiom : ObjectPropertyAxiom
    {
        public ObjectPropertyExpression Property { get; protected set; }
    }

    public class EquivalentObjectPropertiesAxiom : ObjectPropertyAxiom
    {
        public IEnumerable<ObjectPropertyExpression> EquivalentProperties { get; protected set; }
    }

    public class DisjointObjectPropertiesAxiom : ObjectPropertyAxiom
    {
        public IEnumerable<ObjectPropertyExpression> DisjointProperties { get; protected set; }
    }

    public class SubObjectPropertyOfAxion : ObjectPropertyAxiom
    {
        public ObjectPropertyExpression SuperProperty { get; protected set; }

        public IOrderedEnumerable<ObjectPropertyExpression> SubProperties { get; protected set; }
    }

    public class ObjectPropertyDomainAxiom : UnaryObjectPropertyAxiom
    {
        public ClassExpression Domain { get; protected set; }
    }

    public class ObjectPropertyRangeAxiom : UnaryObjectPropertyAxiom
    {
        public ClassExpression Range { get; protected set; }
    }

    public class InverseObjectPropertyAxiom : UnaryObjectPropertyAxiom
    {
        public ObjectPropertyExpression InverseProperty { get; protected set; }
    }

    public class FunctionalObjectPropertyAxiom : UnaryObjectPropertyAxiom
    {
    }

    public class ReflexiveObjectPropertyAxiom : UnaryObjectPropertyAxiom
    {
    }

    public class InverseFunctionalObjectPropertyAxiom : UnaryObjectPropertyAxiom
    {
    }

    public class IrreflexiveObjectPropertyAxiom : UnaryObjectPropertyAxiom
    {
    }

    public class SymmetricObjectPropertyAxiom : UnaryObjectPropertyAxiom
    {
    }

    public class TransitiveObjectPropertyAxiom : UnaryObjectPropertyAxiom
    {
    }

    public class AsymmetricObjectPropertyAxiom : UnaryObjectPropertyAxiom
    {
    }
}
