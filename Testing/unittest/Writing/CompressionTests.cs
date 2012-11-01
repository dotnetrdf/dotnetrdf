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
using VDS.RDF.Writing;

namespace VDS.RDF.Test.Writing
{
    public abstract class CompressionTests
    {
        List<KeyValuePair<IRdfWriter, IRdfReader>> _compressers = new List<KeyValuePair<IRdfWriter, IRdfReader>>()
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
                Assert.AreEqual(g, h, "Graphs should be equal after round trip to and from serialization using " + kvp.Key.GetType().Name);
            }
        }
    }
}
