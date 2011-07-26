using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using VDS.RDF;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Storage;
using VDS.Alexandria;

namespace alexloader
{
    public class AlexandriaLoader
    {
        private String _configFile;
        private String _storeID;
        private bool _debug = false;
        private bool _quiet = false;
        private List<String> _inputs = new List<string>();

        public void Run(String[] args)
        {
            if (args.Length == 0)
            {
                ShowUsage();
            }
            else
            {
                //Try and set the Options
                if (!SetOptions(args))
                {
                    Console.Error.WriteLine("alexloader: Error: One or more required command line options were not set/were invalid");
                    return;
                }

                //First try to load in the Configuration File
                Graph config = new Graph();
                try
                {
                    FileLoader.Load(config, this._configFile);
                    ConfigurationLoader.AutoDetectObjectFactories(config);
                    if (!this._quiet) Console.WriteLine("alexloader: Loaded the Configuration File OK");
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("alexloader: Error: Unable to load the Configuration Graph from the File " + this._configFile + " due to the following error:");
                    Console.Error.WriteLine(ex.Message);
                    DebugErrors(ex);
                    return;
                }

                //Then see if there is a Store ID specified
                INode storeNode;
                try
                {
                    if (this._storeID != null)
                    {
                        if (this._storeID.StartsWith("_:"))
                        {
                            storeNode = config.GetBlankNode(this._storeID);
                        }
                        else
                        {
                            storeNode = config.GetUriNode(new Uri(this._storeID));
                        }
                        if (storeNode == null)
                        {
                            throw new Exception("The given Store ID '" + this._storeID + "' does not exist in the Configuration File");
                        }
                    }
                    else
                    {
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

                            this._storeID = Console.ReadLine();
                            if (Int32.TryParse(this._storeID, out i))
                            {
                                storeNode = ts.Skip(i).Select(t => t.Subject).FirstOrDefault();
                                if (storeNode == null)
                                {
                                    throw new Exception("The selection '" + this._storeID + "' did not correspond to an available Store");
                                }
                            }
                            else
                            {
                                throw new Exception("The selection '" + this._storeID + "' did not correspond to an available Store");
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

                if (!this._quiet) Console.WriteLine("alexloader: Selected the Store to use OK");

                //Now try and load this Store as an IGenericIOManager
                IGenericIOManager manager;
                try
                {
                    Object temp = ConfigurationLoader.LoadObject(config, storeNode);
                    if (temp is IGenericIOManager)
                    {
                        manager = (IGenericIOManager)temp;
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

                if (!this._quiet) Console.WriteLine("alexloader: Loaded the connection to the selected Store OK");

                //Start parsing the inputs
                if (this._inputs.Count > 0)
                {
                    Stopwatch parseTimer = new Stopwatch();
                    Stopwatch loadTimer = new Stopwatch();
                    FileLoader.Warning += new RdfReaderWarning(ShowWarnings);
                    FileLoader.StoreWarning += new StoreReaderWarning(ShowWarnings);

                    foreach (String input in this._inputs)
                    {
                        try
                        {
                            //Assume a Graph format first
                            IRdfReader reader = MimeTypesHelper.GetParser(MimeTypesHelper.GetMimeType(Path.GetExtension(input)));

                            if (!this._quiet) Console.WriteLine("alexloader: Attempting to use the " + reader.GetType().Name + " to parse input file " + input);
                            Graph g = new Graph();
                            parseTimer.Start();
                            FileLoader.Load(g, input);
                            parseTimer.Stop();
                            if (!this._quiet) Console.WriteLine("alexloader: Parsed " + input + " OK - Total Parsing Time so far is " + parseTimer.Elapsed);

                            if (!this._quiet) Console.WriteLine("alexloader: Attempting to save to Store");
                            loadTimer.Start();
                            manager.SaveGraph(g);
                            loadTimer.Stop();
                            Console.WriteLine("alexloader: Loaded " + input + " OK - Total Load Time so far is " + loadTimer.Elapsed);

                        }
                        catch (RdfParserSelectionException)
                        {
                            //Otherwise assume a Store
                            IStoreReader reader = MimeTypesHelper.GetStoreParser(MimeTypesHelper.GetMimeType(Path.GetExtension(input)));

                            if (!this._quiet) Console.WriteLine("alexloader: Attempting to use the " + reader.GetType().Name + " to parse input file " + input);
                            TripleStore store = new TripleStore();
                            parseTimer.Start();
                            FileLoader.Load(store, input);
                            parseTimer.Stop();
                            if (!this._quiet) Console.WriteLine("alexloader: Parsed " + input + " OK - Total Parsing Time so far is " + parseTimer.Elapsed);

                            if (!this._quiet) Console.WriteLine("alexloader: Attempting to save to Store");
                            loadTimer.Start();
                            foreach (IGraph g in store.Graphs)
                            {
                                manager.SaveGraph(g);
                            }
                            loadTimer.Stop();
                            Console.WriteLine("alexloader: Loaded " + input + " OK - Total Load Time so far is " + loadTimer.Elapsed);
                        }
                        catch (Exception ex)
                        {
                            parseTimer.Stop();
                            loadTimer.Stop();
                            Console.Error.WriteLine("alexloader: Error: Unable to load data from file '" + input + "' due to the following error:");
                            Console.Error.WriteLine(ex.Message);
                            DebugErrors(ex);
                        }
                        Console.WriteLine();
                    }

                    if (manager is BaseAlexandriaManager)
                    {
                        Console.WriteLine("alexloader: Issuing the Flush() command to the Store to ensure any outstanding writes are completed");
                        loadTimer.Start();
                        ((BaseAlexandriaManager)manager).Flush();
                        loadTimer.Stop();
                        Console.WriteLine("alexloader: Flush() completed OK - Total Load Time so far is " + loadTimer.Elapsed);
                    }
                    else if (manager is ISqlIOManager)
                    {
                        Console.WriteLine("alexloader: Issuing the Flush() command to the Store to ensure any outstanding writes are completed");
                        loadTimer.Start();
                        ((ISqlIOManager)manager).Flush();
                        loadTimer.Stop();
                        Console.WriteLine("alexloader: Flush() completed OK - Total Load Time so far is " + loadTimer.Elapsed);
                    }

                    Console.WriteLine();
                    if (!this._quiet) Console.WriteLine("alexloader: Issuing the Dispose() command on the connection");
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

        private void ShowWarnings(String message)
        {
            Console.WriteLine("alexloader: Warning: " + message);
        }

        private bool SetOptions(String[] args)
        {
            //Minimum of 2 arguments required
            if (args.Length < 2) return false;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Equals("-config"))
                {
                    if (i < args.Length - 1)
                    {
                        this._configFile = args[i + 1];
                        i++;
                    }
                    else
                    {
                        Console.Error.WriteLine("alexloader: Error: Expected a filename after the -config argument but there were no further arguments");
                        return false;
                    }
                }
                else if (args[i].Equals("-store"))
                {
                    if (i < args.Length - 1)
                    {
                        this._storeID = args[i + 1];
                        i++;
                    }
                    else
                    {
                        Console.Error.WriteLine("alexloader: Warning: Expected a Store ID after the -store argument but there were no further arguments - this option was ignored");
                    }
                }
                else if (args[i].Equals("-debug"))
                {
                    Console.WriteLine("alexloader: Debug mode enabled, verbose errors messages will be produced");
                    this._debug = true;
                }
                else if (args[i].Equals("-quiet"))
                {
                    this._quiet = true;
                }
                else
                {
                    this._inputs.Add(args[i]);
                }
            }

            if (this._configFile == null)
            {
                Console.Error.WriteLine("alexloader: Error: Required -config argument was not found");
                return false;
            }
            else
            {
                return true;
            }
        }

        private void ShowUsage()
        {
            Console.WriteLine("Alexandria Loader");
            Console.WriteLine("=================");
            Console.WriteLine();
            Console.WriteLine("Usage is alexloader -config config.ttl [-store id] file1.rdf [file2.rdf [...]] [Options]");
            Console.WriteLine();
            Console.WriteLine("The required -config config.ttl argument specifies the configuration file which defines connections to the Store you wish to load into");
            Console.WriteLine("The optional -store argument specifies a URI/Blank Node ID for the Store you wish to use");
            Console.WriteLine("The id parameter should either be of the form _:id or http://some.uri");
            Console.WriteLine();
            Console.WriteLine("Supported Options");
            Console.WriteLine("-----------------");
            Console.WriteLine();
            Console.WriteLine("-debug");
            Console.WriteLine("  Specifies that more detailed error output should be produced in the event of errors");
            Console.WriteLine();
            Console.WriteLine("-quiet");
            Console.WriteLine("  Specifies that limited information messages should be output by the loader");
        }

        private void DebugErrors(Exception ex)
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
