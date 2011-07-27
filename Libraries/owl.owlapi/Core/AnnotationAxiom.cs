using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.OWL
{
    public abstract class AnnotationAxiom : Axiom
    {
    }

    public abstract class UnaryAnnotationAxiom : AnnotationAxiom
    {
        public AnnotationProperty Property { get; protected set; }
    }

    public class SubAnnotationPropertyOfAxiom : AnnotationAxiom
    {
        public AnnotationProperty SubProperty { get; protected set; }

        public AnnotationProperty SuperProperty { get; protected set; }
    }

    public class AnnotationPropertyDomainAxiom : UnaryAnnotationAxiom
    {
        public Iri Domain { get; protected set; }
    }

    public class AnnotationPropertyRangeAxiom : UnaryAnnotationAxiom
    {
        public Iri Range { get; protected set; }
    }

    public class AnnotationAssertionAxiom : UnaryAnnotationAxiom
    {
        public IAnnotationSubject Subject { get; protected set; }

        public IAnnotationValue Value { get; protected set; }
    }
}
