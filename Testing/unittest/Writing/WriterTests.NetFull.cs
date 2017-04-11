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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Xunit;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Writing
{
    public partial class WriterTests
    {
        [Fact]
        public void WritingQNameValidation()
        {
            Graph g = new Graph();
            g.NamespaceMap.AddNamespace("ex", new Uri("http://example.org/"));
            INode subj = g.CreateUriNode("ex:subject");
            INode pred = g.CreateUriNode("ex:predicate");
            List<INode> objects = new List<INode>()
            {
                g.CreateUriNode("ex:123"),
                g.CreateBlankNode("a_blank_node"),
                g.CreateBlankNode("_blank"),
                g.CreateBlankNode("-blank"),
                g.CreateBlankNode("123blank"),
                g.CreateUriNode("ex:_object"),
                g.CreateUriNode("ex:-object")
            };
            foreach (INode obj in objects)
            {
                g.Assert(subj, pred, obj);
            }

            this.CheckCompressionRoundTrip(g);
        }
    }
}
