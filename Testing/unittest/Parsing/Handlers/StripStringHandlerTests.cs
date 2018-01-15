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
using System.IO;
using Xunit;

namespace VDS.RDF.Parsing.Handlers
{
    public partial class StripStringHandlerTests
    {
        [Fact]
        public void ThrowsOnBadInstantiation()
        {
            Assert.Throws<ArgumentNullException>(() => new StripStringHandler(null));
        }

        [Fact]
        public void StripsStringDatatype()
        {
            var graphA = Load("<http://example.com/subject> <http://example.com/predicate> \"object\".");
            var graphB = Load("<http://example.com/subject> <http://example.com/predicate> \"object\"^^<http://www.w3.org/2001/XMLSchema#string>.");

            var diff = graphA.Difference(graphB);

            Assert.True(diff.AreEqual, "Graphs should be equal.");
        }

        [Fact]
        public void DoesntStripOtherDatatype()
        {
            var graphA = Load("<http://example.com/subject> <http://example.com/predicate> \"object\".");
            var graphB = Load("<http://example.com/subject> <http://example.com/predicate> \"object\"^^<http://example.com/schema/string>.");

            var diff = graphA.Difference(graphB);

            Assert.False(diff.AreEqual, "Graphs should be different.");
        }

        [Fact]
        public void WorksWithApply()
        {
            var originalRDF = "<http://example.com/subject> <http://example.com/predicate> \"object\"^^<http://www.w3.org/2001/XMLSchema#string>.";
            var referenceRDF = "<http://example.com/subject> <http://example.com/predicate> \"object\".";

            using (var originalGraph = new Graph())
            {
                originalGraph.LoadFromString(originalRDF);

                using (var resultGraph = new Graph())
                {
                    new StripStringHandler(new GraphHandler(resultGraph)).Apply(originalGraph);

                    using (var referenceGraph = new Graph())
                    {
                        referenceGraph.LoadFromString(referenceRDF);

                        Assert.True(resultGraph.Difference(referenceGraph).AreEqual, "Graphs should be equal.");
                    }
                }
            }
        }

        [Fact]
        public void DoesntConfuseFragmentUris()
        {
            var originalRDF = "<http://example.com/subject> <http://example.com/predicate> \"0\"^^<http://www.w3.org/2001/XMLSchema#integer>.";

            using (var originalGraph = new Graph())
            {
                originalGraph.LoadFromString(originalRDF);

                using (var resultGraph = new Graph())
                {
                    new StripStringHandler(new GraphHandler(resultGraph)).Apply(originalGraph);

                    Assert.True(resultGraph.Difference(originalGraph).AreEqual, "Graphs should be equal.");
                }
            }
        }

        private static IGraph Load(string source)
        {
            var result = new Graph();
            var graphHandler = new GraphHandler(result);
            var strippingHandler = new StripStringHandler(graphHandler);
            var parser = new NTriplesParser();

            using (var reader = new StringReader(source))
            {
                parser.Load(strippingHandler, reader);
            }

            return result;
        }
    }
}
