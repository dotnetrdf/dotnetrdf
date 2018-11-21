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

            var result = ExecuteDiff(older, newer);

            Assert.Equal(newer, result);
        }

        [Fact]
        public void Removed_blanks_are_filtered_and_converted_to_variables()
        {
            var older = GraphFrom("_:s <urn:p> <urn:o> .");
            var newer = new Graph();

            var result = ExecuteDiff(older, newer);

            Assert.Equal(newer, result);
        }

        [Fact]
        public void Added_triples_are_added_to_insert()
        {
            var older = new Graph();
            var newer = GraphFrom(@"
<urn:s> <urn:p> <urn:o> .
_:s <urn:p> <urn:o> .
");

            var result = ExecuteDiff(older, newer);

            Assert.Equal(newer, result);
        }

        [Fact]
        public void Supports_named_graph_in_with_clause()
        {
            var older = new Graph();
            older.BaseUri = new Uri("urn:g");

            var newer = GraphFrom("<urn:s> <urn:p> <urn:o> .");

            var result = ExecuteDiff(older, newer, older.BaseUri);

            Assert.Equal(newer, result);
        }

        [Fact]
        public void Null_RHS_is_all_removals()
        {
            var older = GraphFrom("<urn:s> <urn:p> <urn:o> .");

            var result = ExecuteDiff(older, null);

            Assert.True(result.IsEmpty);
        }

        [Fact]
        public void Null_LHS_is_all_additions()
        {
            var newer = GraphFrom("<urn:s> <urn:p> <urn:o> .");

            var result = ExecuteDiff(null, newer);

            Assert.Equal(newer, result);
        }

        [Fact]
        public void Difference_between_nulls_is_NOOP()
        {
            var result = ExecuteDiff(null, null);

            Assert.True(result.IsEmpty);
        }

        private static IGraph GraphFrom(string rdf)
        {
            var g = new Graph();
            g.LoadFromString(rdf);

            return g;
        }

        private IGraph ExecuteDiff(IGraph older, IGraph newer, Uri graphUri = null)
        {
            var diff = new GraphDiff().Difference(older, newer);
            var update = diff.AsUpdate(graphUri);
            var sparql = update.ToString();

            output.WriteLine(sparql);

            older = older ?? new Graph();

            var ts = new TripleStore();
            ts.Add(older);
            
            var store = new InMemoryManager(ts) as IUpdateableStorage;
            store.Update(sparql);

            return older;
        }
    }
}
