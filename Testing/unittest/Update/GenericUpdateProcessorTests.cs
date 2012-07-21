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
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Storage;
using VDS.RDF.Update;

namespace VDS.RDF.Test.Update
{
    [TestClass]
    public abstract class GenericUpdateProcessorTests
    {
        private SparqlUpdateParser _parser = new SparqlUpdateParser();

        protected abstract IStorageProvider GetManager();

        [TestMethod]
        public void SparqlUpdateGenericCreateAndInsertData()
        {
            IStorageProvider manager = this.GetManager();
            GenericUpdateProcessor processor = new GenericUpdateProcessor(manager);
            SparqlUpdateCommandSet cmds = this._parser.ParseFromString("CREATE GRAPH <http://example.org/sparqlUpdate/created>; INSERT DATA { GRAPH <http://example.org/sparqlUpdate/created> { <http://example.org/s> <http://example.org/p> <http://example.org/o> } }");

            processor.ProcessCommandSet(cmds);

            Graph g = new Graph();
            manager.LoadGraph(g, "http://example.org/sparqlUpdate/created");

            TestTools.ShowGraph(g);

            Assert.IsFalse(g.IsEmpty, "[" + manager.ToString() + "] Graph should not be empty");
            Assert.AreEqual(1, g.Triples.Count, "[" + manager.ToString() + "] Graph should have 1 Triple");
        }

        [TestMethod]
        public void SparqlUpdateGenericCreateInsertDeleteData()
        {
            IStorageProvider manager = this.GetManager();
            GenericUpdateProcessor processor = new GenericUpdateProcessor(manager);
            SparqlUpdateCommandSet cmds = this._parser.ParseFromString("CREATE GRAPH <http://example.org/sparqlUpdate/created>; INSERT DATA { GRAPH <http://example.org/sparqlUpdate/created> { <http://example.org/s> <http://example.org/p> <http://example.org/o> } }");

            processor.ProcessCommandSet(cmds);

            Graph g = new Graph();
            manager.LoadGraph(g, "http://example.org/sparqlUpdate/created");

            TestTools.ShowGraph(g);

            Assert.IsFalse(g.IsEmpty, "[" + manager.ToString() + "] Graph should not be empty");
            Assert.AreEqual(1, g.Triples.Count, "[" + manager.ToString() + "] Graph should have 1 Triple");

            cmds = this._parser.ParseFromString("DELETE DATA { GRAPH <http://example.org/sparqlUpdate/created> { <http://example.org/s> <http://example.org/p> <http://example.org/o> } }");
            processor.ProcessCommandSet(cmds);

            Graph h = new Graph();
            manager.LoadGraph(h, "http://example.org/sparqlUpdate/created");

            TestTools.ShowGraph(h);

            Assert.IsTrue(h.IsEmpty, "[" + manager.ToString() + "] Graph should be empty");
        }
    }
}
