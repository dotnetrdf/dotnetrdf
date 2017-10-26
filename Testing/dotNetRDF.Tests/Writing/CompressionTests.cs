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
using VDS.RDF.Writing;

namespace VDS.RDF.Writing
{
    public abstract class CompressionTests
    {
        readonly List<KeyValuePair<IRdfWriter, IRdfReader>> _compressers = new List<KeyValuePair<IRdfWriter, IRdfReader>>()
        {
            new KeyValuePair<IRdfWriter, IRdfReader>(new CompressingTurtleWriter(), new TurtleParser()),
            new KeyValuePair<IRdfWriter, IRdfReader>(new CompressingTurtleWriter(TurtleSyntax.W3C), new TurtleParser(TurtleSyntax.W3C)),
            new KeyValuePair<IRdfWriter, IRdfReader>(new Notation3Writer(), new Notation3Parser()),
            new KeyValuePair<IRdfWriter, IRdfReader>(new RdfXmlWriter(), new RdfXmlParser()),
            new KeyValuePair<IRdfWriter, IRdfReader>(new PrettyRdfXmlWriter(), new RdfXmlParser())
        };

        protected void CheckCompressionRoundTrip(IGraph g)
        {
            foreach (KeyValuePair<IRdfWriter, IRdfReader> kvp in this._compressers)
            {

                IRdfWriter writer = kvp.Key;
                if (writer is ICompressingWriter)
                {
                    ((ICompressingWriter)writer).CompressionLevel = WriterCompressionLevel.High;
                }
                if (writer is IHighSpeedWriter)
                {
                    ((IHighSpeedWriter)writer).HighSpeedModePermitted = false;
                }
                System.IO.StringWriter strWriter = new System.IO.StringWriter();
                writer.Save(g, strWriter);

                Console.WriteLine("Compressed Output using " + kvp.Key.GetType().Name);
                Console.WriteLine(strWriter.ToString());
                Console.WriteLine();

                Graph h = new Graph();
                StringParser.Parse(h, strWriter.ToString(), kvp.Value);

                GraphDiffReport report = g.Difference(h);
                if (!report.AreEqual) TestTools.ShowDifferences(report);
                Assert.Equal(g, h);
            }
        }
    }
}
