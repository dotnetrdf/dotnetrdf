using System;
using System.Collections.Generic;

namespace VDS.OWL
{
    public interface IOntology
    {
        BaseAnnotationCollection Annotations 
        { 
            get;
        }

        BaseAxiomCollection Axioms 
        { 
            get; 
        }

        IEnumerable<Iri> Imports 
        {
            get;
        }

        Iri OntologyIri 
        {
            get;
        }

        Iri VersionIri 
        {
            get;
        }
    }
}
