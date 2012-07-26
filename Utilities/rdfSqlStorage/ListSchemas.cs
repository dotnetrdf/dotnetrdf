using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.Data.Sql.Clients.Cmd
{
    public class ListSchemas
        : BaseManagementAction
    {
        public ListSchemas()
            : base("listschemas", "Lists available built-in database schemas") { }

        public override int MinimumArguments
        {
            get 
            {
                return 0; 
            }
        }

        public override void ShowUsage()
        {
            Console.WriteLine("rdfSqlStorage (listschemas mode)");
            Console.WriteLine("================================");
            Console.WriteLine();
            Console.WriteLine("Lists available built-in database schemas and their descriptions");
            Console.WriteLine();
            Console.WriteLine("Usage is rdfSqlStorage listschemas");
        }

        public override void Run(string[] args)
        {
            Console.WriteLine("rdfSqlStorage: There are " + AdoSchemaHelper.SchemaDefinitions.Count() + " built-in schemas available:");
            foreach (AdoSchemaDefinition def in AdoSchemaHelper.SchemaDefinitions)
            {
                Console.WriteLine(def.Name + " -> " + def.Description);
            }
        }
    }
}
