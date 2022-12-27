DROP GRAPH <http://example.org/sesame/core-374>;
INSERT DATA
{ 
  GRAPH <http://example.org/sesame/core-374>
  {
  <http://poolpartyw.csintra.net/FIRD/CountryCd> <http://www.w3.org/2004/02/skos/core#hasTopConcept> <http://poolpartyw.csintra.net/FIRD/CountryCd/NE> ;
     a <http://www.w3.org/2004/02/skos/core#ConceptScheme> ;
     rdfs:label "CountryCd"@en ;
     <http://purl.org/dc/terms/title> "CountryCd"@en .
  <http://poolpartyw.csintra.net/FIRD/CountryCd/NE> <http://www.w3.org/2004/02/skos/core#inScheme> <http://poolpartyw.csintra.net/FIRD/CountryCd> ;
     a <http://www.w3.org/2004/02/skos/core#Concept> ;
     <http://www.w3.org/2004/02/skos/core#prefLabel> "Republik Niger"@de ;
     <http://www.w3.org/2004/02/skos/core#prefLabel> "République du Niger"@fr ;
     <http://www.w3.org/2004/02/skos/core#prefLabel> "Repubblica del Niger"@it ;
     <http://www.w3.org/2004/02/skos/core#prefLabel> "Republic of Niger"@en ;
     <http://www.w3.org/2004/02/skos/core#notation> "NE" .
  }
};