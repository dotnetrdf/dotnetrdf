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
            new KeyValuePair<IRdfWriter, IRdfReader>(new FastRdfXmlWriter(), new RdfXmlParser()),
            new KeyValuePair<IRdfWriter, IRdfReader>(new Notation3Writer(), new Notation3Parser()),
            //new KeyValuePair<IRdfWriter, IRdfReader>(new RdfXmlTreeWriter(), new RdfXmlParser()),
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

                Assert.AreEqual(g, h, "Graphs should be equal after round trip to and from serialization using " + kvp.Key.GetType().Name);
            }
        }
    }
}
