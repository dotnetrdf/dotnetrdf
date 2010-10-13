using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using VDS.Alexandria;
using VDS.RDF;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Storage;

namespace alexloader
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                ShowUsage();
            }
            else
            {
                int firstInputArg;

                //First try to load in the Configuration File
                Graph config = new Graph();
                try
                {
                    FileLoader.Load(config, args[0]);
                    ConfigurationLoader.AutoDetectObjectFactories(config);
                    Console.WriteLine("alexloader: Loaded the Configuration File OK");
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("alexloader: Error: Unable to load the Configuration Graph from the File " + args[0] + " due to the following error:");
                    Console.Error.WriteLine(ex.Message);
                    DebugErrors(ex);
                    return;
                }

                //Then see if there is a Store ID specified
                INode storeNode;
                try
                {
                    if (args[1].Equals("-store"))
                    {
                        firstInputArg = 3;
                        String storeID = args[2];
                        if (storeID.StartsWith("_:"))
                        {
                            storeNode = config.GetBlankNode(storeID);
                        }
                        else
                        {
                            storeNode = config.GetUriNode(new Uri(storeID));
                        }
                        if (storeNode == null)
                        {
                            throw new Exception("The given Store ID '" + storeID + "' does not exist in the Configuration File");
                        }
                    }
                    else
                    {
                        firstInputArg = 1;
                        //Ask User which Generic IO Manager to use
                        INode rdfType = config.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));
                        INode genericIOManager = ConfigurationLoader.CreateConfigurationNode(config, ConfigurationLoader.ClassGenericManager);

                        IEnumerable<Triple> ts = config.GetTriplesWithPredicateObject(rdfType, genericIOManager);
                        if (ts.Any())
                        {
                            int i = 0;
                            Console.WriteLine("alexloader: Select which Store you would like to load into:");
                            foreach (Triple t in ts)
                            {
                                Console.WriteLine(i + ": " + t.Subject.ToString());
                                i++;
                            }

                            String storeID = Console.ReadLine();
                            if (Int32.TryParse(storeID, out i))
                            {
                                storeNode = ts.Skip(i).Select(t => t.Subject).FirstOrDefault();
                                if (storeNode == null)
                                {
                                    throw new Exception("The selection '" + storeID + "' did not correspond to an available Store");
                                }
                            }
                            else
                            {
                                throw new Exception("The selection '" + storeID + "' did not correspond to an available Store");
                            }

                        }
                        else
                        {
                            throw new Exception("There are no Generic IO Managers defined in your Configuration file");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("alexloader: Error: Unable to select a Store to load into from the given Configuration File, the file must specify at least one GenericIOManager and the -store parameter if used must point to a valid URI/Blank Node in the Configuration File");
                    Console.Error.WriteLine(ex.Message);
                    DebugErrors(ex);
                    return;
                }

                Console.WriteLine("alexloader: Selected the Store to use OK");

                //Now try and load this Store as an IGenericIOManager - and more specifically a BaseAlexandriaManager
                BaseAlexandriaManager manager;
                try
                {
                    Object temp = ConfigurationLoader.LoadObject(config, storeNode);
                    if (temp is IGenericIOManager)
                    {
                        if (temp is BaseAlexandriaManager)
                        {
                            manager = (BaseAlexandriaManager)temp;
                        }
                        else
                        {
                            throw new Exception("The selected Store was not loadable as an Object which implements the BaseAlexandriaManager interface");
                        }
                    }
                    else
                    {
                        throw new Exception("The selected Store was not loadable as an Object which implements the IGenericIOManager interface");
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("alexloader: Error: Unable to load the connection to the selected Store due to the following error:");
                    Console.Error.WriteLine(ex.Message);
                    DebugErrors(ex);
                    return;
                }

                Console.WriteLine("alexloader: Loaded the connection to the selected Store OK");

                //Start parsing the inputs
                if (firstInputArg < args.Length)
                {
                    Stopwatch parseTimer = new Stopwatch();
                    Stopwatch loadTimer = new Stopwatch();
                    FileLoader.Warning += new RdfReaderWarning(ShowWarnings);
                    FileLoader.StoreWarning += new StoreReaderWarning(ShowWarnings);

                    for (int i = firstInputArg; i < args.Length; i++)
                    {
                        try
                        {
                            //Assume a Graph format first
                            IRdfReader reader = MimeTypesHelper.GetParser(MimeTypesHelper.GetMimeType(Path.GetExtension(args[i])));

                            Console.WriteLine("alexloader: Attempting to use the " + reader.GetType().Name + " to parse input file " + args[i]);
                            Graph g = new Graph();
                            parseTimer.Start();
                            FileLoader.Load(g, args[i]);
                            parseTimer.Stop();
                            Console.WriteLine("alexloader: Parsed " + args[i] + " OK - Total Parsing Time so far is " + parseTimer.Elapsed);

                            Console.WriteLine("alexloader: Attempting to save to Store");
                            loadTimer.Start();
                            manager.SaveGraph(g);
                            loadTimer.Stop();
                            Console.WriteLine("alexloader: Loaded " + args[i] + " OK - Total Load Time so far is " + loadTimer.Elapsed);

                        }
                        catch (RdfParserSelectionException)
                        {
                            //Otherwise assume a Store
                            IStoreReader reader = MimeTypesHelper.GetStoreParser(MimeTypesHelper.GetMimeType(Path.GetExtension(args[i])));

                            Console.WriteLine("alexloader: Attempting to use the " + reader.GetType().Name + " to parse input file " + args[i]);
                            TripleStore store = new TripleStore();
                            parseTimer.Start();
                            FileLoader.Load(store, args[i]);
                            parseTimer.Stop();
                            Console.WriteLine("alexloader: Parsed " + args[i] + " OK - Total Parsing Time so far is " + parseTimer.Elapsed);

                            Console.WriteLine("alexloader: Attempting to save to Store");
                            loadTimer.Start();
                            foreach (IGraph g in store.Graphs)
                            {
                                manager.SaveGraph(g);
                            }
                            loadTimer.Stop();
                            Console.WriteLine("alexloader: Loaded " + args[i] + " OK - Total Load Time so far is " + loadTimer.Elapsed);
                        }
                        catch (Exception ex)
                        {
                            parseTimer.Stop();
                            loadTimer.Stop();
                            Console.Error.WriteLine("alexloader: Error: Unable to load data from file '" + args[i] + "' due to the following error:");
                            Console.Error.WriteLine(ex.Message);
                            DebugErrors(ex);
                        }
                        Console.WriteLine();
                    }

                    Console.WriteLine("alexloader: Issuing the Flush() command to the Store to ensure any outstanding writes are completed");
                    loadTimer.Start();
                    manager.Flush();
                    loadTimer.Stop();
                    Console.WriteLine("alexloader: Flush() completed OK - Total Load Time so far is " + loadTimer.Elapsed);

                    Console.WriteLine();
                    Console.WriteLine("alexloader: Issuing the Dispose() command on the connection");
                    loadTimer.Start();
                    manager.Dispose();
                    loadTimer.Stop();

                    Console.WriteLine("alexloader: Loading Completed - Total Parsing Time was " + parseTimer.Elapsed + " - Total Load Time was " + loadTimer.Elapsed);
                }
                else
                {
                    Console.WriteLine("alexloader: Nothing to do - no input files were specified");
                }
            }
        }

        static void ShowWarnings(String message)
        {
            Console.WriteLine("alexloader: Warning: " + message);
        }

        static void ShowUsage()
        {
            Console.WriteLine("Alexandria Loader");
            Console.WriteLine("=================");
            Console.WriteLine();
            Console.WriteLine("Usage is alexloader config.ttl [-store id] file1.rdf [file2.rdf [...]]");
            Console.WriteLine();
            Console.WriteLine("The optional -store argument specifies a URI/Blank Node ID for the Store Manager you wish to use");
            Console.WriteLine("The id parameter should either be of the form _:id or http://some.uri");
        }

        static void DebugErrors(Exception ex)
        {
            Console.WriteLine(ex.StackTrace);

            while (ex.InnerException != null)
            {
                ex = ex.InnerException;
                Console.WriteLine();
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

    }
}
