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
using System.Threading;
using System.IO;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace dotNetRDFTest
{
    public class ThreadSafeGraphTest
    {
        private ThreadSafeGraph _g = new ThreadSafeGraph();
        private int _repeats = 100;
        private int _threads = 2;

        public ThreadSafeGraphTest()
        {

        }

        public ThreadSafeGraphTest(int threads, int repeats)
        {
            this._threads = threads;
            this._repeats = repeats;
        }

        public void RunTest()
        {
            StreamWriter output = new StreamWriter("ThreadSafeGraphTest.txt");
            try {
                Console.SetOut(output);

                Console.WriteLine("## Thread Safe Graph Test");
                Console.WriteLine("# Testing with " + this._threads + " Threads and " + this._repeats + " Repeats");
                Console.WriteLine();

                List<Thread> threads = new List<Thread>();
                for (int i = 0; i < this._threads; i++)
                {
                    Thread t = new Thread(new ThreadStart(Test));
                    threads.Add(t);
                }

                foreach (Thread t in threads)
                {
                    t.Start();
                }

                //Wait for Finish
                while (true)
                {
                    int activeThreads = 0;
                    foreach (Thread t in threads)
                    {
                        if (t.ThreadState != ThreadState.Stopped)
                        {
                            activeThreads++;
                        }

                        //Do some reading of the Graph directly from Triples property
                        IUriNode thread = this._g.CreateUriNode(new Uri("http://threads.org/" + t.ManagedThreadId));
                        int count = 0;
                        foreach (Triple tri in this._g.Triples)
                        {
                            if (tri.Subject.Equals(thread))
                            {
                                count++;
                            }
                        }
                        Console.WriteLine("Currently there are " + count + " Triples in the Graph produced by Thread #" + t.ManagedThreadId);
                        Console.WriteLine("Currently there are " + this._g.Nodes.Count() + " Nodes in the Graph");
                    }

                    if (activeThreads > 0)
                    {
                        Thread.Sleep(100);
                    }
                    else
                    {
                        break;
                    }
                }

                //Output the end result
                Console.WriteLine();
                Console.WriteLine("Final Graph contains the following Triples");
                Console.WriteLine();
                foreach (Triple t in this._g.Triples)
                {
                    Console.WriteLine(t.ToString());
                }

            } finally {
                output.Close();
            }
        }

        private void Test()
        {
            IUriNode thread = this._g.CreateUriNode(new Uri("http://threads.org/" + Thread.CurrentThread.ManagedThreadId));
            IUriNode label = this._g.CreateUriNode("rdfs:label");
            Uri dateTimeType = new Uri(XmlSpecsHelper.XmlSchemaDataTypeDateTime);

            try
            {
                for (int i = 0; i < this._repeats; i++)
                {
                    //Assert a couple of things
                    ILiteralNode now = this._g.CreateLiteralNode(DateTime.Now.ToString(XmlSpecsHelper.XmlSchemaDateTimeFormat), dateTimeType);
                    this._g.Assert(new Triple(thread, label, now));
                    this._g.Assert(new Triple(thread, label, this._g.CreateLiteralNode("Repeat #" + i)));

                    //Select all the stuff we've added
                    foreach (Triple t in this._g.GetTriplesWithSubject(thread))
                    {
                        //Spin spin spin...
                    }

                    //Retract the date time stamp triple
                    this._g.Retract(new Triple(thread, label, now));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in Thread #" + Thread.CurrentThread.ManagedThreadId);
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);

                if (ex.InnerException != null) {
                    Console.WriteLine(ex.InnerException.Message);
                    Console.WriteLine(ex.InnerException.StackTrace);
                }
            }
        }

    }
}
