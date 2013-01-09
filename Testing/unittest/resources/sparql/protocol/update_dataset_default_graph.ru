PREFIX dc: <http://purl.org/dc/terms/>
PREFIX foaf: <http://xmlns.com/foaf/0.1/>
CLEAR ALL ;
INSERT DATA {
    GRAPH <http://kasei.us/2009/09/sparql/data/data1.rdf> {
        <http://kasei.us/2009/09/sparql/data/data1.rdf> a foaf:Document
    }
} ;
INSERT {
    GRAPH <http://example.org/protocol-update-dataset-test/> {
        ?s a dc:BibliographicResource
    }
}
USING <http://kasei.us/2009/09/sparql/data/data1.rdf>
WHERE {
    ?s a foaf:Document
}