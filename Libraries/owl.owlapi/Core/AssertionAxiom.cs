using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.OWL
{
    public abstract class AssertionAxiom : Axiom
    {
    }

    public class SameIndividualAxiom : AssertionAxiom
    {
        public IEnumerable<IIndividual> SameIndividuals { get; protected set; }
    }

    public class DifferentIndividualsAxiom : AssertionAxiom
    {
        public IEnumerable<IIndividual> DifferentIndividuals { get; protected set; }
    }

    public class ClassAssertionAxiom : AssertionAxiom
    {
        public IIndividual Individual { get; protected set; }

        public ClassExpression Class { get; protected set; }
    }

    public class ObjectPropertyAssertionAxiom : AssertionAxiom
    {
        public IIndividual Target { get; protected set; }

        public IIndividual Source { get; protected set; }

        public ObjectPropertyExpression Property { get; protected set; }
    }

    public class NegativeObjectPropertyAssertionAxiom : AssertionAxiom
    {
        public IIndividual Target { get; protected set; }

        public IIndividual Source { get; protected set; }

        public ObjectPropertyExpression Property { get; protected set; }
    }

    public class DataPropertyAssertion : AssertionAxiom
    {
        public IIndividual Source { get; protected set; }

        public DataPropertyExpression Property { get; protected set; }

        public ILiteral TargetValue { get; protected set; }
    }

    public class NegativeDataPropertyAssertion : AssertionAxiom
    {
        public IIndividual Source { get; protected set; }

        public DataPropertyExpression Property { get; protected set; }

        public ILiteral TargetValue { get; protected set; }
    }
}
