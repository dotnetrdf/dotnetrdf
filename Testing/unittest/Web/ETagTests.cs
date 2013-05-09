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
using System.Diagnostics;
using System.Linq;
using System.Text;
using NUnit.Framework;
using VDS.RDF.Parsing;
using VDS.RDF.Web;

namespace VDS.RDF.Web
{
    [TestFixture]
    public class ETagTests
    {
        [Test]
        public void WebETagComputation()
        {
            try
            {
                Graph g = new Graph();
                FileLoader.Load(g, "resources\\InferenceTest.ttl");

                Stopwatch timer = new Stopwatch();
                timer.Start();
                String etag = g.GetETag();
                timer.Stop();
                Console.WriteLine("ETag is " + etag);
                Console.WriteLine(etag.Length);
                Console.WriteLine("Took " + timer.Elapsed + " to calculate");
                Console.WriteLine();

                Assert.IsFalse(String.IsNullOrEmpty(etag));
                Assert.AreEqual(40, etag.Length, "Expected ETag Length was 40 characters");

                timer.Reset();
                timer.Start();
                String etag2 = g.GetETag();
                timer.Stop();
                Console.WriteLine("ETag is " + etag2);
                Console.WriteLine("Took " + timer.Elapsed + " to calculate");
                Console.WriteLine();

                Assert.AreEqual(etag, etag2, "ETags should be consistent unless the Graph changes");
                Assert.AreEqual(40, etag2.Length, "Expected ETag Length was 40 characters");

                g.Retract(g.Triples.First());
                timer.Reset();
                timer.Start();
                String etag3 = g.GetETag();
                timer.Stop();
                Console.WriteLine("Graph has now been altered so ETag should change");
                Console.WriteLine("ETag is " + etag3);
                Console.WriteLine("Took " + timer.Elapsed + " to calculate");
                Console.WriteLine();

                Assert.AreNotEqual(etag, etag3, "ETags should change if the Graph changes");
                Assert.AreNotEqual(etag2, etag3, "ETags should change if the Graph changes");
                Assert.AreEqual(40, etag3.Length, "Expected ETag Length was 40 characters");
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
