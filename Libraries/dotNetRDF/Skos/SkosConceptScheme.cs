namespace VDS.RDF.Skos
{
    using System.Collections.Generic;

    public class SkosConceptScheme : SkosResource
    {
        public SkosConceptScheme(INode resource) : base(resource) { }

        public IEnumerable<SkosConcept> HasTopConcept
        {
            get
            {
                return this.GetConcepts(SkosHelper.HasTopConcept);
            }
        }
    }
}
