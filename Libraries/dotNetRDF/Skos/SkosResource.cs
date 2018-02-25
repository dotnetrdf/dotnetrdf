namespace VDS.RDF.Skos
{
    using System.Collections.Generic;
    using System.Linq;

    public abstract class SkosResource
    {
        protected INode resource;

        protected internal SkosResource(INode resource)
        {
            this.resource = resource ?? throw new RdfSkosException("Cannot create a SKOS Resource for a null Resource");
        }

        protected IEnumerable<SkosConceptScheme> GetConceptSchemes(string predicateUri)
        {
            return this
                .GetObjects(predicateUri)
                .Select(o => new SkosConceptScheme(o));
        }

        protected IEnumerable<SkosConcept> GetConcepts(string predicateUri)
        {
            return this
                .GetObjects(predicateUri)
                .Select(o => new SkosConcept(o));
        }

        protected IEnumerable<ILiteralNode> GetLiterals(string predicateUri)
        {
            return this
                .GetObjects(predicateUri)
                .Cast<ILiteralNode>();
        }

        protected IEnumerable<INode> GetObjects(string predicateUri)
        {
            var predicate = this.resource.Graph
                .CreateUriNode(
                    UriFactory.Create(
                        predicateUri));

            return this.resource.Graph
                .GetTriplesWithSubjectPredicate(this.resource, predicate)
                .Select(t => t.Object);
        }
    }
}
