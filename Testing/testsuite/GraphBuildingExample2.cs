using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF;
using VDS.RDF.Writing;

namespace dotNetRDFTest
{
    class GraphBuildingExample2
    {

        public static void Main(string[] args)
        {
            //Create a new Empty Graph
            Graph g = new Graph();

            //Define Namespaces
            g.NamespaceMap.AddNamespace("pets", new Uri("http://example.org/pets"));
            
            //Create Uri Nodes
            IUriNode dog, fido, rob, owner, name, species, breed, lab;
            dog = g.CreateUriNode("pets:Dog");
            fido = g.CreateUriNode("pets:abc123");
            rob = g.CreateUriNode("pets:def456");
            owner = g.CreateUriNode("pets:hasOwner");
            name = g.CreateUriNode("pets:hasName");
            species = g.CreateUriNode("pets:isAnimal");
            breed = g.CreateUriNode("pets:isBreed");
            lab = g.CreateUriNode("pets:Labrador");

            //Assert Triples
            g.Assert(new Triple(fido, species, dog));
            g.Assert(new Triple(fido, owner, rob));
            g.Assert(new Triple(fido, name, g.CreateLiteralNode("Fido")));
            g.Assert(new Triple(rob, name, g.CreateLiteralNode("Rob")));
            g.Assert(new Triple(fido, breed, lab));

            //Attempt to output GraphViz
            try
            {
                Console.WriteLine("Writing GraphViz DOT file graph_building_example2.dot");
                GraphVizWriter gvzwriter = new GraphVizWriter();
                gvzwriter.Save(g, "graph_building_example2.dot");

                Console.WriteLine("Creating a PNG got this Graph called graph_building_example2.png");
                GraphVizGenerator gvzgen = new GraphVizGenerator("svg", "C:\\Program Files (x86)\\Graphviz2.20\\bin");
                gvzgen.Format = "png";
                gvzgen.Generate(g, "graph_building_example2.png", false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}
