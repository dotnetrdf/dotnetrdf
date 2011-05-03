using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Writing.Formatting;

namespace sparqlDown
{
    class Program
    {
        static void Main(string[] args)
        {
            String query = @"PREFIX rdfs:    <http://www.w3.org/2000/01/rdf-schema#>
    PREFIX wn:  <http://www.w3.org/2006/03/wn/wn20/schema/>
     
    SELECT DISTINCT ?label WHERE
    {
     {
       ?s1 wn:memberMeronymOf <http://wordnet.rkbexplorer.com/id/synset-solar_system-noun-1> .
       ?s1 rdfs:label ?label.
     }
     MINUS
     {
      ?s2 wn:hyponymOf <http://wordnet.rkbexplorer.com/id/synset-Roman_deity-noun-1> .
      ?s2 rdfs:label ?label.
     }
     BIND(URI(CONCAT('http://dbpedia.org/resource/', ?label)) AS ?dbpResource)
     FILTER(EXISTS
     {
       SERVICE <http://dbpedia.org/sparql>
       {
         ?dbpResource a <http://dbpedia.org/ontology/Planet> .
       }
     })
    }";

            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);


            SparqlFormatter formatter = new SparqlFormatter(q.NamespaceMap);
            Console.WriteLine("Original Query");
            Console.WriteLine(formatter.Format(q));
            Console.WriteLine();

            ISparqlAlgebra algebra = q.ToAlgebra();
            Console.WriteLine("Original Algebra");
            Console.WriteLine(algebra.ToString());
            Console.WriteLine();

            SparqlDownOptimiser optimiser = new SparqlDownOptimiser(new Uri("http://wordnet.rkbexplorer.com/sparql"));
            algebra = optimiser.Optimise(algebra);

            Console.WriteLine("Optimised Algebra");
            Console.WriteLine(algebra.ToString());
            Console.WriteLine();

            SparqlQuery q2 = algebra.ToQuery();
            Console.WriteLine("Resulting Query");
            Console.WriteLine(formatter.Format(q2));

            Console.ReadKey();

        }
    }
}
