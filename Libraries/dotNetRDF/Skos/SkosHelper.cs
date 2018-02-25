namespace VDS.RDF.Skos
{
    public static class SkosHelper
    {
        public const string Prefix = "skos";
        public const string Namespace = "http://www.w3.org/2004/02/skos/core#";

        public const string Concept = Namespace + "Concept";
        public const string ConceptScheme = Namespace + "ConceptScheme";

        public const string InScheme = Namespace + "inScheme";
        public const string HasTopConcept = Namespace + "hasTopConcept";
        public const string TopConceptOf = Namespace + "topConceptOf";

        public const string PrefLabel = Namespace + "prefLabel";
        public const string AltLabel = Namespace + "altLabel";
        public const string HiddenLabel = Namespace + "hiddenLabel";

        public const string Notation = Namespace + "notation";

        public const string Note = Namespace + "note";
        public const string ChangeNote = Namespace + "changeNote";
        public const string Definition = Namespace + "definition";
        public const string EditorialNote = Namespace + "editorialNote";
        public const string Example = Namespace + "example";
        public const string HistoryNote = Namespace + "historyNote";
        public const string ScopeNote = Namespace + "scopeNote";

        public const string SemanticRelation = Namespace + "semanticRelation";
        public const string Broader = Namespace + "broader";
        public const string Narrower = Namespace + "narrower";
        public const string Related = Namespace + "related";
        public const string BroaderTransitive = Namespace + "broaderTransitive";
        public const string NarrowerTransitive = Namespace + "narrowerTransitive";

        public const string Collection = Namespace + "Collection";
        public const string OrderedCollection = Namespace + "OrderedCollection";
        public const string Member = Namespace + "member";

        public const string MappingRelation = Namespace + "mappingRelation";
        public const string CloseMatch = Namespace + "closeMatch";
        public const string ExactMatch = Namespace + "exactMatch";
        public const string BroadMatch = Namespace + "broadMatch";
        public const string NarrowMatch = Namespace + "narrowMatch";
        public const string RelatedMatch = Namespace + "relatedMatch";
    }
}
