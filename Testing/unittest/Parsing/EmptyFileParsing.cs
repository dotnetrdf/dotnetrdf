/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

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
