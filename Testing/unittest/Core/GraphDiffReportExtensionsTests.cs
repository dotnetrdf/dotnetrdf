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

using VDS.RDF.Storage;
using Xunit;
using Xunit.Abstractions;

namespace VDS.RDF
{
    public class GraphDiffReportExtensionsTests
    {
        private readonly ITestOutputHelper output;

        public GraphDiffReportExtensionsTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void Removed_triples_are_added_to_delete_and_where()
        {
            var older = GraphFrom("<urn:s> <urn:p> <urn:o> .");
            var newer = new Graph();

            ExecuteDiff(older, newer);

            Assert.Equal(newer, older);
        }

        [Fact]
        public void Removed_blanks_are_filtered_and_converted_to_variables()
        {
            var older = GraphFrom("_:s <urn:p> <urn:o> .");
            var newer = new Graph();

            ExecuteDiff(older, newer);

            Assert.Equal(newer, older);
        }

        [Fact]
        public void Added_triples_are_added_to_insert()
        {
            var older = new Graph();
            var newer = GraphFrom(@"
<urn:s> <urn:p> <urn:o> .
_:s <urn:p> <urn:o> .
");

            ExecuteDiff(older, newer);

            Assert.Equal(newer, older);
        }

        private static IGraph GraphFrom(string rdf)
        {
            var g = new Graph();
            g.LoadFromString(rdf);

            return g;
        }

        private void ExecuteDiff(IGraph older, IGraph newer)
        {
            var ts = new TripleStore();
            ts.Add(older);

            var store = new InMemoryManager(ts) as IUpdateableStorage;

            var diff = older.Difference(newer);
            var update = diff.AsUpdate();
            var sparql = update.ToString();

            output.WriteLine(sparql);

            store.Update(sparql);
        }
    }
}
