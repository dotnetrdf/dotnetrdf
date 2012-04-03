using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;

namespace VDS.RDF.Test.Parsing
{
     [TestClass]
    public class EmptyFileParsing
    {
         private void TestEmptyParsing(IRdfReader reader)
         {
             if (!File.Exists("empty.test"))
             {
                 FileStream temp = File.Create("empty.test");
                 temp.Close();
             }

             Graph g = new Graph();
             reader.Load(g, "empty.test");

             Assert.IsTrue(g.IsEmpty, "Graph should be empty");
         }

         private void TestEmptyDatasetParsing(IStoreReader reader)
         {
             if (!File.Exists("empty.test"))
             {
                 FileStream temp = File.Create("empty.test");
                 temp.Close();
             }

             TripleStore store = new TripleStore();
             reader.Load(store, "empty.test");

             Assert.AreEqual(0, store.Graphs.Count, "Store should have no Graphs");
         }
         
         [TestMethod]
         public void ParsingEmptyFileNTriples()
         {
             this.TestEmptyParsing(new NTriplesParser());
         }

         [TestMethod]
         public void ParsingEmptyFileTurtle()
         {
             this.TestEmptyParsing(new TurtleParser());
         }

         [TestMethod]
         public void ParsingEmptyFileNotation3()
         {
             this.TestEmptyParsing(new Notation3Parser());
         }

         [TestMethod]
         public void ParsingEmptyFileNQuads()
         {
             this.TestEmptyDatasetParsing(new NQuadsParser());
         }

         [TestMethod]
         public void ParsingEmptyFileTriG()
         {
             this.TestEmptyDatasetParsing(new TriGParser());
         }
    }
}
