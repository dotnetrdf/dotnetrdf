/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

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
