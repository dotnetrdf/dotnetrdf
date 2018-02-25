namespace VDS.RDF.Skos
{
    using System.Linq;
    using VDS.RDF.Parsing;

    public class SkosMember : SkosResource
    {
        public SkosMember(INode resource) : base(resource) { }

        public static SkosMember Create(INode node)
        {
            var a = node.Graph.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType));
            var typeStatements = node.Graph.GetTriplesWithSubjectPredicate(node, a);

            var skosOrderedCollection = node.Graph.CreateUriNode(UriFactory.Create(SkosHelper.OrderedCollection));
            if (typeStatements.WithObject(skosOrderedCollection).Any())
            {
                return new SkosOrderedCollection(node);
            }

            var skosCollection = node.Graph.CreateUriNode(UriFactory.Create(SkosHelper.Collection));
            if (typeStatements.WithObject(skosCollection).Any())
            {
                return new SkosCollection(node);
            }

            return new SkosConcept(node);
        }
    }
}
