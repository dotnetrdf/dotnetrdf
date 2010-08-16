using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.IO;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;
using VDS.RDF.Storage;
using VDS.RDF.Storage.Params;

namespace dotNetRDFTest
{
    public class FolderStoreTest
    {
        public static void Main(string[] args)
        {
            StreamWriter output = new StreamWriter("FolderStoreTest.txt");
            try
            {
                //Set Output
                Console.SetOut(output);

                Console.WriteLine("## Folder Store Test Suite");
                Console.WriteLine();
                Console.WriteLine("# Loading a Triple Store from MS SQL to use as test for Folder Store");

                //Get a Triple Store
                ITripleStore store = new SqlTripleStore("dotnetrdf_experimental","sa","20sQl08");

                Console.WriteLine("# Triple Store Loading Done");
                Console.WriteLine();

                //Show the Triples
                Console.WriteLine("# Following Triples are in the Store");
                foreach (Triple t in store.Triples)
                {
                    Console.WriteLine(t.ToString(true) + " from Graph " + t.GraphUri.ToString());
                }

                //Write to a Folder
                Console.WriteLine();
                Console.WriteLine("# Attempting to write the Store to a Folder");
                Console.WriteLine("Starting @ " + DateTime.Now.ToString(TestSuite.TestSuiteTimeFormat));
                Stopwatch timer = new Stopwatch();
                timer.Start();

                FolderStoreWriter storewriter = new FolderStoreWriter();
                storewriter.Save(store, new FolderStoreParams("folderstore_test",FolderStoreFormat.Turtle));

                timer.Stop();
                Console.WriteLine("Finished @ " + DateTime.Now.ToString(TestSuite.TestSuiteTimeFormat));
                Console.WriteLine("Writing took " + timer.ElapsedMilliseconds + "ms");
                Console.WriteLine("# Written to " + System.IO.Path.GetFullPath("folderstore_test"));
                
                Console.WriteLine();

                //Read from a Folder
                Console.WriteLine("# Attempting to read back into a new Store from the Folder");
                Console.WriteLine("Starting @ " + DateTime.Now.ToString(TestSuite.TestSuiteTimeFormat));
                timer.Reset();
                timer.Start();

                TripleStore newstore = new TripleStore();
                FolderStoreReader storereader = new FolderStoreReader();
                storereader.Load(newstore, new FolderStoreParams("folderstore_test", FolderStoreFormat.AutoDetect));

                timer.Stop();
                Console.WriteLine("Finished @ " + DateTime.Now.ToString(TestSuite.TestSuiteTimeFormat));
                Console.WriteLine("Reading took " + timer.ElapsedMilliseconds + "ms");

                Console.WriteLine();

                //Show the Triples
                Console.WriteLine("# Following Triples are in the Store");
                foreach (Triple t in store.Triples)
                {
                    Console.WriteLine(t.ToString(true) + " from Graph " + t.GraphUri.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }

            output.Close();
        }
    }
}
