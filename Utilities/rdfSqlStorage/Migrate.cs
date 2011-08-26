using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.Data.Sql.Clients.Cmd
{
    public class Migrate
        : ManagementAction
    {
        private bool _verify = false, _halt = false;

        public Migrate()
            : base("migrate", "Migrates a legacy SQL Store to the current ADO store format") { }

        public override int MinimumArguments
        {
            get 
            {
                return 1; 
            }
        }

        public override void ShowUsage()
        {
            Console.WriteLine("rdfSqlStorage (migrate mode)");
            Console.WriteLine("============================");
            Console.WriteLine();
            Console.WriteLine("Permits the migration from the old legacy SQL store format to the new ADO store format");
            Console.WriteLine();
            Console.WriteLine("Usage is rdfSqlStorage migrate migrateConfig.ttl [options]");
            Console.WriteLine();
            Console.WriteLine("See SampleMigrationConfig.ttl for an example configuration file for migration");
            Console.WriteLine();
            Console.WriteLine("Supported Options");
            Console.WriteLine("-----------------");
            Console.WriteLine();
            Console.WriteLine("-halt");
            Console.WriteLine(" Used in conjunction with -verify option to set that migration should -halt if verification fails");
            Console.WriteLine();
            Console.WriteLine("-verify");
            Console.WriteLine(" Performs extra verification that the migrated data is identical to the original data");
        }

        public override void Run(string[] args)
        {
            if (args.Length < 2)
            {
                this.ShowUsage();
                return;
            }

            //Check Configuration File is OK
            String config = args[1];
            if (args[1].Equals("-help"))
            {
                this.ShowUsage();
                return;
            }
            if (!File.Exists(config))
            {
                Console.Error.WriteLine("rdfSqlStorage: Error: Specified migration configuration file '" + config + "' does not exist!");
                return;
            }

            //Check for other options
            this.CheckOptions(args);

            try
            {
                Graph g = new Graph();
                g.LoadFromFile(config);
                Console.WriteLine("rdfSqlStorage: Read migration configuration file OK...");

                ConfigurationLoader.AutoDetectObjectFactories(g);

                INode sourceNode = g.GetUriNode(new Uri("dotnetrdf:migration:source"));
                if (sourceNode == null)
                {
                    Console.Error.WriteLine("rdfSqlStorage: Error: Expected to find the source for the migration specified by the special URI <dotnetrdf:migration:source> but it does not exist in the given Configuration File!");
                    return;
                }
                INode targetNode = g.GetUriNode(new Uri("dotnetrdf:migration:target"));
                if (targetNode == null)
                {
                    Console.Error.WriteLine("rdfSqlStorage: Error: Expected to find the target for the migration specified by the special URI <dotnetrdf:migration:target> but it does not exist in the given Configuration File!");
                    return;
                }

                //Try to load the Objects
                Object sourceObj = ConfigurationLoader.LoadObject(g, sourceNode);
                if (!(sourceObj is ISqlIOManager))
                {
                    Console.Error.WriteLine("rdfSqlStorage: Error: Expected the migration source to be loadable as an object of type ISqlIOManager!");
                    return;
                }
                Object targetObj = ConfigurationLoader.LoadObject(g, targetNode);
                if (!(targetObj is IGenericIOManager))
                {
                    Console.Error.WriteLine("rdfSqlStorage: Error: Expected the migration target to be loadable as an object of type IGenericIOManager!");
                    return;
                }
                Console.WriteLine("rdfSqlStorage: Loaded migration source and target OK...");
                
                //Retrieve Graph list from Migration Source
                ISqlIOManager source = (ISqlIOManager)sourceObj;
                Console.WriteLine("rdfSqlStorage: Retrieving Graph URIs from migration source...");
                try
                {
                    source.Open(true);
                    List<Uri> uris = source.GetGraphUris();
                    Console.WriteLine("rdfSqlStorage: Migration source has " + uris.Count + " Graph URIs");

                    //Start migrating Graphs
                    IGenericIOManager target = (IGenericIOManager)targetObj;
                    for (int i = 0; i < uris.Count; i++)
                    {
                        Console.WriteLine("rdfSqlStorage: Migrating Graph '" + uris[i].ToSafeString() + "' (" + (i + 1) + " of " + uris.Count + ")...");

                        String id = source.GetGraphID(uris[i]);

                        using (Graph temp = new Graph())
                        {
                            temp.BaseUri = uris[i];
                            Console.WriteLine("rdfSqlStorage: Loading Graph into memory...");
                            source.LoadTriples(temp, id);
                            Console.WriteLine("rdfSqlStorage: Loaded " + temp.Triples.Count + " Triple(s)");

                            Console.WriteLine("rdfSqlStorage: Saving Graph to migration target...");
                            target.SaveGraph(temp);
                            Console.WriteLine("rdfSqlStorage: Saved Graph OK");

                            if (this._verify)
                            {
                                Console.WriteLine("rdfSqlStorage: Retrieving newly saved Graph from migration target to verify data migration...");
                                using (Graph temp2 = new Graph())
                                {
                                    target.LoadGraph(temp2, uris[i]);
                                    Console.WriteLine("rdfSqlStorage: Retrieved newly saved Graph OK, proceeding to verify data...");

                                    GraphDiffReport report = temp.Difference(temp2);
                                    if (!report.AreEqual)
                                    {
                                        Console.Error.WriteLine("rdfSqlStorage: Warning: Data Verification failed: " + report.AddedTriples.Count() + " incorrect Ground Triple(s) and " + report.AddedMSGs.Count() + " incorrect MSGs containing " + report.AddedMSGs.Sum(x => x.Triples.Count) + " non-Ground Triple(s)");
                                        if (this._halt)
                                        {
                                            Console.Error.WriteLine("rdfSqlStorage: Error: Halting due to data verification failure!");
                                            return;
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("rdfSqlStorage: Data Verified OK!");
                                    }
                                    temp2.Dispose();
                                }
                            }

                            temp.Dispose();
                        }
                        GC.GetTotalMemory(true);
                        Console.WriteLine();
                    }

                    Console.WriteLine("rdfSqlStorage: Finishing Migrating Graphs!");
                }
                finally
                {
                    source.Close(true);
                }
            }
            catch (RdfParserSelectionException selEx)
            {
                Console.Error.WriteLine("rdfSqlStorage: Error: Specified migration configuration file is not in a RDF format that the tool understands!");
                this.PrintErrorTrace(selEx);
            }
            catch (RdfParseException parseEx)
            {
                Console.Error.WriteLine("rdfSqlStorage: Error: Specified migration configuration file is not valid RDF!");
                this.PrintErrorTrace(parseEx);
            }
            catch (DotNetRdfConfigurationException configEx)
            {
                Console.Error.WriteLine("rdfSqlStorage: Error: Specified migration configuration file contains malformed configuration information!");
                this.PrintErrorTrace(configEx);
            }
        }

        private void CheckOptions(String[] args)
        {
            if (args.Length >= 3)
            {
                for (int i = 2; i < args.Length; i++)
                {
                    String arg = args[i];
                    switch (arg)
                    {
                        case "-verify":
                            this._verify = true;
                            break;

                        case "-halt":
                            this._halt = true;
                            break;

                        default:
                            Console.Error.WriteLine("rdfSqlStorage: Warning: Unknown Option " + arg + " ignored");
                            break;
                    }
                }
            }
        }
    }
}
