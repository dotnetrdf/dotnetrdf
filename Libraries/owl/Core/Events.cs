using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.OWL
{
    public class AxiomEventArgs : EventArgs
    {
        public AxiomEventArgs(IOntology ontology, IAxiom axiom, bool retracted)
        {
            this.Ontology = ontology;
            this.Axiom = axiom;
            this.WasAsserted = !retracted;
            this.WasRetracted = retracted;
        }

        public AxiomEventArgs(IOntology ontology, IAxiom axiom)
            : this(ontology, axiom, false) { }

        public IAxiom Axiom { get; private set; }

        public IOntology Ontology { get; private set; }

        public bool WasAsserted { get; private set; }

        public bool WasRetracted { get; private set; }
    }

    public delegate void AxiomEventHandler(Object sender, AxiomEventArgs args);

    public class AnnotationEventArgs : EventArgs
    {
        public AnnotationEventArgs(IOntology ontology, IAnnotation annotation, bool retracted)
        {
            this.Ontology = ontology;
            this.Annotation = annotation;
            this.WasAsserted = !retracted;
            this.WasRetracted = retracted;
        }

        public AnnotationEventArgs(IOntology ontology, IAnnotation annotation)
            : this(ontology, annotation, false) { }

        public IAnnotation Annotation { get; private set; }

        public IOntology Ontology { get; private set; }

        public bool WasAsserted { get; private set; }

        public bool WasRetracted { get; private set; }
    }

    public delegate void AnnotationEventHandler(Object sender, AnnotationEventArgs args);
}
