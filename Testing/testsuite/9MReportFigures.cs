using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;

namespace dotNetRDFTest
{
    public class NineMonthReportFigures
    {
        public static void Main(string[] args)
        {
            Graph g = new Graph();
            Graph h = new Graph();
            UriLoader.Load(g, new Uri("http://nottm.ecs.soton.ac.uk/AllAboutThatWeb/profiles/exports/14"));
            UriLoader.Load(h, new Uri("http://nottm.ecs.soton.ac.uk/AllAboutThatWeb/profiles/14"));

            //Output to GraphViz
            GraphVizGenerator gvizgen = new GraphVizGenerator("png");
            gvizgen.Generate(g, "Triple.png", false);
            gvizgen.Generate(h, "AATTriple.png", false);

            GraphVizWriter gvizwriter = new GraphVizWriter();
            gvizwriter.Save(g, "Triple.dot");
            gvizwriter.Save(h, "AATTriple.dot");
        }
    }
}
