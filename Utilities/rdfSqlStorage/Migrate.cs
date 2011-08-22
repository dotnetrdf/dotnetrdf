using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            throw new NotImplementedException();
        }
    }
}
