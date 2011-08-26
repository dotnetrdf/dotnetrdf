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
            try
            {
                Graph g = new Graph();
                g.LoadFromFile(config);

                ConfigurationLoader.AutoDetectObjectFactories(g);
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
    }
}
