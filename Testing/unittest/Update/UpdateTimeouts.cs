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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Update;

namespace VDS.RDF.Test.Update
{
    [TestClass]
    public class UpdateTimeouts
    {
        private SparqlUpdateParser _parser = new SparqlUpdateParser();

        [TestMethod]
        public void SparqlUpdateTimeout()
        {
            String update = "CREATE GRAPH <http://example.org/1>; LOAD <http://www.dotnetrdf.org/configuration#>; CREATE GRAPH <http://example.org/2>";
            SparqlUpdateCommandSet cmds = this._parser.ParseFromString(update);
            cmds.Timeout = 1;

            TripleStore store = new TripleStore();
            LeviathanUpdateProcessor processor = new LeviathanUpdateProcessor(store);
            try
            {
                processor.ProcessCommandSet(cmds);
                Assert.Fail("Expected a SparqlUpdateTimeoutException");
            }
            catch (SparqlUpdateTimeoutException timeoutEx)
            {
                TestTools.ReportError("Timeout", timeoutEx);
                Console.WriteLine();
                Console.WriteLine("Execution Time: " + cmds.UpdateExecutionTime.Value.ToString());

                Assert.IsFalse(store.HasGraph(new Uri("http://example.org/1")), "Graph 1 should not exist");
                Assert.IsFalse(store.HasGraph(new Uri("http://example.org/2")), "Graph 2 should not exist");

            }
        }
    }
}
