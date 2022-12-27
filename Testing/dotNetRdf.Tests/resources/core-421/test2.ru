INSERT  
{
  GRAPH <http://test.org/user> 
  {
    <http://test.org/user> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://schema.org/Person> .
    <http://test.org/user> <http://some/ontology/favorite> <http://test.org/product/name> .
  }
  
  GRAPH <http://test.org/prodList/> 
  {
    <http://test.org/user> <http://xmlns.com/foaf/0.1/primaryTopic> <http://test.org/user> .
  }
}
WHERE { }