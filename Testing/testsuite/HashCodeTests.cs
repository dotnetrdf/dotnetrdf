using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;
using VDS.RDF;
using VDS.RDF.Storage;

namespace dotNetRDFTest
{
    public class HashCodeTests
    {

        public static void Main(string[] args)
        {
            StreamWriter output = new StreamWriter("HashCodeTests.txt");
            Console.SetOut(output);

            try
            {
                Console.WriteLine("## Hash Code Tests");
                Console.WriteLine("Tests that Literal and URI Nodes produce different Hashes");
                Console.WriteLine();

                //Create the Nodes
                Graph g = new Graph();
                UriNode u = g.CreateUriNode(new Uri("http://www.google.com"));
                LiteralNode l = g.CreateLiteralNode("http://www.google.com/");

                Console.WriteLine("Created a URI and Literal Node both referring to 'http://www.google.com'");
                Console.WriteLine("String form of URI Node is:");
                Console.WriteLine(u.ToString());
                Console.WriteLine("String form of Literal Node is:");
                Console.WriteLine(l.ToString());
                Console.WriteLine("Hash Code of URI Node is " + u.GetHashCode());
                Console.WriteLine("Hash Code of Literal Node is " + l.GetHashCode());
                Console.WriteLine("Hash Codes are Equal? " + u.GetHashCode().Equals(l.GetHashCode()));
                Console.WriteLine("Nodes are equal? " + u.Equals(l));

                //Create Triples
                BlankNode b = g.CreateBlankNode();
                UriNode type = g.CreateUriNode("rdf:type");
                Triple t1, t2;
                t1 = new Triple(b, type, u);
                t2 = new Triple(b, type, l);

                Console.WriteLine();
                Console.WriteLine("Created two Triples stating a Blank Node has rdf:type of the Nodes created earlier");
                Console.WriteLine("String form of Triple 1 (using URI Node) is:");
                Console.WriteLine(t1.ToString());
                Console.WriteLine("String form of Triple 2 (using Literal Node) is:");
                Console.WriteLine(t2.ToString());
                Console.WriteLine("Hash Code of Triple 1 is " + t1.GetHashCode());
                Console.WriteLine("Hash Code of Triple 2 is " + t2.GetHashCode());
                Console.WriteLine("Hash Codes are Equal? " + t1.GetHashCode().Equals(t2.GetHashCode()));
                Console.WriteLine("Triples are Equal? " + t1.Equals(t2));

                //Now going to look at the Hash Code collisions from the dotNetRDF Store
                Console.WriteLine();
                Console.WriteLine("Examing the Hash Code Collisions from one of our SQL Store test data sets");

//                MicrosoftSqlStoreManager manager = new MicrosoftSqlStoreManager("localhost", "bbcone", "example", "password");
//                DataTable collisions = manager.ExecuteQuery(@"SELECT * FROM TRIPLES WHERE tripleHash IN 
//	(
//	SELECT tripleHash FROM TRIPLES GROUP BY tripleHash HAVING COUNT(tripleID)>1
//	) ORDER BY tripleHash");

//                foreach (DataRow r in collisions.Rows)
//                {
//                    String s, p, o;
//                    int hash;
//                    s = r["tripleSubject"].ToString();
//                    p = r["triplePredicate"].ToString();
//                    o = r["tripleObject"].ToString();
//                    hash = Int32.Parse(r["tripleHash"].ToString());

//                    INode subj = manager.LoadNode(g, s);
//                    INode pred = manager.LoadNode(g, p);
//                    INode obj = manager.LoadNode(g, o);

//                    Triple t = new Triple(subj, pred, obj);

//                    Console.WriteLine("Subject (ID " + s + "): " + subj.ToString() + " (Hash " + subj.GetHashCode() + ")");
//                    Console.WriteLine("Predicate (ID " + p + "): " + pred.ToString() + " (Hash " + pred.GetHashCode() + ")");
//                    Console.WriteLine("Object (ID " + o + "): " + obj.ToString() + " (Hash " + obj.GetHashCode() + ")");
//                    Console.WriteLine(t.ToString());
//                    Console.WriteLine("Triple Hash " + t.GetHashCode());
//                    Console.WriteLine("Triple Hash Code Construct " + subj.GetHashCode().ToString() + pred.GetHashCode().ToString() + obj.GetHashCode().ToString());
//                    Console.WriteLine("Triple Hash in Store " + hash);
//                    Console.WriteLine();
//                }

//                manager.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
            finally
            {
                output.Close();
            }
        }
    }
}
