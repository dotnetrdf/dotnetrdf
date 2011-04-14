using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.OWL
{
    class Ontology
    {
        public Iri OntologyIri { get; protected set; }

        public Iri VersionIri { get; protected set; }

        public IEnumerable<Iri> Imports { get; protected set; }

        public IAnnotation Annotations { get; protected set; }

        public IAxiom Axioms { get; protected set; }
    }
}
