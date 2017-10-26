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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Update;

namespace VDS.RDF.Update
{
    public class UpdateTimeouts
    {
        private SparqlUpdateParser _parser = new SparqlUpdateParser();

        [Fact(Skip="Remote configuration not currently available")]
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
                Assert.True(false, "Expected a SparqlUpdateTimeoutException");
            }
            catch (SparqlUpdateTimeoutException timeoutEx)
            {
                TestTools.ReportError("Timeout", timeoutEx);
                Console.WriteLine();
                Console.WriteLine("Execution Time: " + cmds.UpdateExecutionTime.Value.ToString());

                Assert.False(store.HasGraph(new Uri("http://example.org/1")), "Graph 1 should not exist");
                Assert.False(store.HasGraph(new Uri("http://example.org/2")), "Graph 2 should not exist");

            }
        }
    }
}
