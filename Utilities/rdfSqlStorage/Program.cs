using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Utilities.Data.Sql.Migrate
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                ShowUsage();
            }
            else
            {
                Console.Error.WriteLine("Not Yet Implemented!");
            }
        }

        private static void ShowUsage()
        {
            Console.WriteLine("rdfSqlMigrate");
            Console.WriteLine("=============");
            Console.WriteLine();
            Console.WriteLine("Migrates from the legacy dotNetRDF SQL store format to the new ADO store format that replaces it");
            Console.WriteLine();
            Console.WriteLine("Usage is rdfSqlMigrate MigrateConfig.ttl [options]");
            Console.WriteLine();
            Console.WriteLine("Please see the included SampleMigrationConfig.ttl file for an example migration configuration file");
            Console.WriteLine();
            Console.WriteLine("Supported Options");
            Console.WriteLine("-----------------");
            Console.WriteLine();
            Console.WriteLine("-verify");
            Console.WriteLine(" Verifies the data after it has been migrated");
        }
    }
}
