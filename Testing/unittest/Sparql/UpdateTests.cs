using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VDS.RDF.Test.Sparql
{
    [TestClass]
    public class UpdateTests
    {
        public const String InsertPatterns1 = @"ex:IndividualA rdf:type          tpl:MyIndividualClass .
_:template        rdf:type          tpl:MyTemplate .
_:template        tpl:ObjectRole    rdl:MyTypeClass .
_:template        tpl:PossessorRole ex:IndividualA .
_:template        tpl:PropertyRole  'ValueA'^^xsd:String .";

        public const String InsertPatterns2 = @"ex:IndividualB rdf:type          tpl:MyIndividualClass .
_:template        rdf:type          tpl:MyTemplate .
_:template        tpl:ObjectRole    rdl:MyTypeClass .
_:template        tpl:PossessorRole ex:IndividualB .
_:template        tpl:PropertyRole  'ValueB'^^xsd:String .";

        [TestMethod]
        public void InsertDataWithBNodes()
        {
            TripleStore store = new TripleStore();
            Graph g = new Graph();
            store.Add(g);

            String prefixes = "PREFIX rdf: <" + NamespaceMapper.RDF + ">\n PREFIX xsd: <" + NamespaceMapper.XMLSCHEMA + ">\n PREFIX ex: <http://example.org/>\n PREFIX rdl: <http://example.org/roles>\n PREFIX tpl: <http://example.org/template/>\n";
            String insert = prefixes + "INSERT DATA { " + InsertPatterns1 + "}";
            Console.WriteLine(insert);
            Console.WriteLine();
            store.ExecuteUpdate(insert);
            insert = prefixes + "INSERT DATA {" + InsertPatterns2 + "}";
            Console.WriteLine(insert);
            Console.WriteLine();
            store.ExecuteUpdate(insert);

            foreach (Triple t in g.Triples)
            {
                Console.WriteLine(t.ToString());
            }
        }
    }
}
