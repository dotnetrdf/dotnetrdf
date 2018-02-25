namespace VDS.RDF.Skos
{
    using System.Collections.Generic;
    using System.Linq;
    using VDS.RDF.Parsing;

    public class SkosGraph : WrapperGraph
    {
        public SkosGraph() : base()
        {
            this.InitializeNamespaceMap();
        }

        public SkosGraph(IGraph g) : base(g)
        {
            this.InitializeNamespaceMap();
        }

        private void InitializeNamespaceMap()
        {
            this.NamespaceMap.AddNamespace(SkosHelper.Prefix, UriFactory.Create(SkosHelper.Namespace));
        }

        public IEnumerable<SkosConceptScheme> ConceptSchemes
        {
            get
            {
                return this.GetInstances(SkosHelper.ConceptScheme).Cast<SkosConceptScheme>();
            }
        }

        public IEnumerable<SkosConcept> Concepts
        {
            get
            {
                return this.GetInstances(SkosHelper.Concept).Cast<SkosConcept>();
            }
        }

        public IEnumerable<SkosCollection> Collections
        {
            get
            {
                return this.GetInstances(SkosHelper.Collection).Cast<SkosCollection>();
            }
        }

        public IEnumerable<SkosOrderedCollection> OrderedCollections
        {
            get
            {
                return this.GetInstances(SkosHelper.OrderedCollection).Cast<SkosOrderedCollection>();
            }
        }

        private IEnumerable<INode> GetInstances(string typeUri)
        {
            var a = this.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType));
            var type = this.CreateUriNode(UriFactory.Create(typeUri));

            return this
                .GetTriplesWithPredicateObject(a, type)
                .Select(t => t.Subject);
        }
    }
}
