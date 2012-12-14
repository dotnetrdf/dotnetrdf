 PREFIX dc: <http://purl.org/dc/terms/>
PREFIX foaf: <http://xmlns.com/foaf/0.1/>
CLEAR ALL ;
INSERT DATA {
    GRAPH <http://kasei.us/2009/09/sparql/data/data1.rdf> { <http://kasei.us/2009/09/sparql/data/data1.rdf> a foaf:Document }
    GRAPH <http://kasei.us/2009/09/sparql/data/data2.rdf> { <http://kasei.us/2009/09/sparql/data/data2.rdf> a foaf:Document }
    GRAPH <http://kasei.us/2009/09/sparql/data/data3.rdf> { <http://kasei.us/2009/09/sparql/data/data3.rdf> a foaf:Document }
} ;
INSERT {
    GRAPH <http://example.org/protocol-update-dataset-named-graphs-test/> {
        ?s a dc:BibliographicResource
    }
}
USING NAMED <http://kasei.us/2009/09/sparql/data/data1.rdf>
USING NAMED <http://kasei.us/2009/09/sparql/data/data2.rdf>
WHERE {
    GRAPH ?g {
        ?s a foaf:Document
    }
}