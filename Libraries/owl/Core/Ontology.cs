using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.OWL
{
    class Ontology : IOntology
    {
        public Iri OntologyIri { get; protected set; }

        public Iri VersionIri { get; protected set; }

        public IEnumerable<Iri> Imports { get; protected set; }

        public BaseAnnotationCollection Annotations { get; protected set; }

        public BaseAxiomCollection Axioms { get; protected set; }
    }
}
