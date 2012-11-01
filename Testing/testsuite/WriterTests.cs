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
using System.IO;
using System.Diagnostics;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;

namespace dotNetRDFTest
{

    public class WriterTests
    {
        public static void reportError(StreamWriter output, String header, Exception ex)
        {
            output.WriteLine(header);
            output.WriteLine(ex.Message);
            output.WriteLine(ex.StackTrace);

            if (!(ex.InnerException == null))
            {
                output.WriteLine(ex.InnerException.Message);
                output.WriteLine(ex.InnerException.StackTrace);
            }
        }

        public static void Main(string[] args)
        {
            StreamWriter output = new StreamWriter("WriterTests.txt");
            Console.SetOut(output);
            Console.WriteLine("## RDF Serialization Tests");

            try
            {
                //Create the Output Directory if required
                if (!Directory.Exists("writer_tests"))
                {
                    Console.WriteLine("Creating Output Directory");
                    Directory.CreateDirectory("writer_tests");
                }

                List<String> testFiles = new List<string>() {
                    "test.n3",
                    "InferenceTest.ttl",
                    "bnodes.ttl"
                };

                //Create list of Writers
                Console.WriteLine("Creating a set of Writer Classes");
                List<IRdfWriter> writers = new List<IRdfWriter>()
                {
                    new NTriplesWriter(),
                    new TurtleWriter(),
                    new CompressingTurtleWriter(),
                    new RdfXmlWriter(),
                    new PrettyRdfXmlWriter(),
                    new Notation3Writer(),
                    new RdfJsonWriter(),
                    new HtmlWriter()
                };

                //Create the Warning Handler
                RdfWriterWarning handler = delegate(String message) { Console.WriteLine("Writer Warning: " + message); };
                writers.ForEach(w => w.Warning += handler);

                //Create list of Compression levels
                List<int> compressionLevels = new List<int>() {
                    WriterCompressionLevel.None,
                    WriterCompressionLevel.Minimal,
                    WriterCompressionLevel.Default,
                    WriterCompressionLevel.Medium,
                    WriterCompressionLevel.More,
                    WriterCompressionLevel.High
                };

                //Create a set of Readers
                Console.WriteLine("Creating a set of Reader Classes");
                List<IRdfReader> readers = new List<IRdfReader>()
                {
                    new NTriplesParser(),
                    new TurtleParser(),
                    new TurtleParser(),
                    new RdfXmlParser(),
                    new RdfXmlParser(),
                    new Notation3Parser(),
                    new RdfJsonParser(),
                    new RdfAParser()
                };

                IRdfReader parser;

                int test = 0;
                foreach (String testFile in testFiles)
                {
                    //Read in the Test Graph
                    //parser = MimeTypesHelper.GetParser(MimeTypesHelper.GetMimeType(testFile));
                    parser = new Notation3Parser();
                    Graph g = new Graph();
                    Console.WriteLine("Attempting to load test file " + testFile);
                    parser.Load(g, testFile);
                    Console.WriteLine("RDF Loaded OK");

                    //Serialize with each parser
                    int i = 0;
                    foreach (IRdfWriter writer in writers)
                    {
                        if (writer is IHighSpeedWriter)
                        {
                            ((IHighSpeedWriter)writer).HighSpeedModePermitted = false;
                        }
                        if (writer is ICompressingWriter)
                        {
                            foreach (int c in compressionLevels)
                            {
                                ((ICompressingWriter)writer).CompressionLevel = c;
                                TestWriter(output, g, writer, readers[i], "tests-" + test + "-" + writer.GetType().ToString() + ".Compression." + c);
                            }
                        }
                        else
                        {
                            TestWriter(output, g, writer, readers[i], "tests-" + test + "-" + writer.GetType().ToString());
                        }

                        i++;
                    }

                    Console.WriteLine();
                    Console.WriteLine(new String('-', 100));
                    test++;
                }
            }
            catch (IOException ioEx)
            {
                reportError(output, "IO Exception", ioEx);
            }
            catch (RdfParseException parseEx)
            {
                reportError(output, "Parsing Exception", parseEx);
            }
            catch (RdfException rdfEx)
            {
                reportError(output, "RDF Exception", rdfEx);
            }
            catch (Exception ex)
            {
                reportError(output, "Other Exception", ex);
            }

            Console.WriteLine("Tests Finished");
            output.Close();
        }

        public static void TestWriter(StreamWriter output, Graph g, IRdfWriter writer, IRdfReader reader, String file)
        {
            Stopwatch timer = new Stopwatch();

            Console.WriteLine();
            Console.WriteLine(new String('-', file.Length));
            Console.WriteLine(file);
            Console.WriteLine("Attempting serialization with " + writer.GetType().ToString());

            //Show Compression Level
            if (writer is ICompressingWriter)
            {
                Console.WriteLine("Compression Level is " + ((ICompressingWriter)writer).CompressionLevel);
            }

            //Enable Pretty Printing if supported
            if (writer is IPrettyPrintingWriter)
            {
                ((IPrettyPrintingWriter)writer).PrettyPrintMode = true;
            }

            try
            {
                timer.Start();
                writer.Save(g, "writer_tests/" + file + ".out");
                Console.WriteLine("Serialization Done");
            }
            catch (IOException ioEx)
            {
                reportError(output, "IO Exception", ioEx);
            }
            catch (RdfParseException parseEx)
            {
                reportError(output, "Parsing Exception", parseEx);
            }
            catch (RdfException rdfEx)
            {
                reportError(output, "RDF Exception", rdfEx);
            }
            catch (Exception ex)
            {
                reportError(output, "Other Exception", ex);
            }
            finally
            {
                timer.Stop();
                Console.WriteLine("Writing took " + timer.ElapsedMilliseconds + "ms");

                //Get the relevant Reader
                Graph h = new Graph();
                try
                {
                    //Read back in the serialized output to check it's valid RDF
                    Console.WriteLine("Attempting to read back in from serialized Output using " + reader.GetType().ToString());
                    reader.Load(h, "writer_tests/" + file + ".out");
                    Console.WriteLine("Serialized Output was valid RDF");

                    //Check same number of Triples are present
                    if (g.Triples.Count == h.Triples.Count)
                    {
                        Console.WriteLine("Correct number of Triples loaded");
                    }
                    else
                    {
                        throw new RdfException("Incorrect number of Triples loaded, got " + h.Triples.Count + " but expected " + g.Triples.Count);
                    }

                    //Check same number of Subjects are present
                    if (g.Triples.SubjectNodes.Distinct().Count() == h.Triples.SubjectNodes.Distinct().Count())
                    {
                        Console.WriteLine("Correct number of Subjects loaded");
                    }
                    else
                    {
                        throw new RdfException("Incorrect number of Subjects loaded, got " + h.Triples.SubjectNodes.Distinct().Count() + " but expected " + g.Triples.SubjectNodes.Distinct().Count());
                    }

                    //Reserialize to NTriples
                    NTriplesWriter ntwriter = new NTriplesWriter();
                    ntwriter.SortTriples = true;
                    ntwriter.Save(h, "writer_tests/" + file + ".nt");
                    Console.WriteLine("Serialized Output reserialized to NTriples");

                    //Check Graphs are Equal
                    if (g.Equals(h))
                    {
                        Console.WriteLine("Graphs are Equal");
                    }
                    else
                    {
                        Console.WriteLine("First Graphs triples:");
                        foreach (Triple t in g.Triples)
                        {
                            Console.WriteLine(t.ToString());
                        }
                        Console.WriteLine();
                        Console.WriteLine("Second Graph triples:");
                        foreach (Triple t in h.Triples)
                        {
                            Console.WriteLine(t.ToString());
                        }
                        Console.WriteLine();
                        throw new RdfException("Graphs are non-equal");
                    }
                }
                catch (IOException ioEx)
                {
                    reportError(output, "IO Exception", ioEx);
                }
                catch (RdfParseException parseEx)
                {
                    reportError(output, "Parsing Exception", parseEx);
                }
                catch (RdfException rdfEx)
                {
                    reportError(output, "RDF Exception", rdfEx);
                }
                catch (Exception ex)
                {
                    reportError(output, "Other Exception", ex);
                }
                Console.WriteLine();
            }
        }
    }


}
