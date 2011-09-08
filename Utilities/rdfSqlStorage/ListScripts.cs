using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.Data.Sql.Clients.Cmd
{
    public class ListScripts
        : BaseManagementAction
    {
        public ListScripts()
            : base("listscripts", "Lists available scripts for a built-in database schema") { }

        public override int MinimumArguments
        {
            get 
            {
                return 1; 
            }
        }

        public override void ShowUsage()
        {
            Console.WriteLine("rdfSqlStorage (listscripts mode)");
            Console.WriteLine("================================");
            Console.WriteLine();
            Console.WriteLine("Lists available scripts for a built-in database schemas");
            Console.WriteLine();
            Console.WriteLine("Usage is rdfSqlStorage listscripts schema");
        }

        public override void Run(string[] args)
        {
            if (args.Length < 2)
            {
                this.ShowUsage();
                return;
            }
            if (args[1].Equals("-help"))
            {
                this.ShowUsage();
                return;
            }

            String schema = args[1];
            AdoSchemaDefinition def = AdoSchemaHelper.SchemaDefinitions.Where(d => d.Name.Equals(schema)).FirstOrDefault();
            if (def == null)
            {
                Console.Error.WriteLine("rdfSqlStorage: Error: There is no built-in schema named '" + schema + "', use the listschemas mode to view available built-in schemas");
                return;
            }
            else
            {
                Console.WriteLine("rdfSqlStorage: There are " + def.ScriptDefinitions.Count() + " scripts available for the '" + schema + "' schema:");
                foreach (AdoSchemaScriptDefinition scriptDef in def.ScriptDefinitions)
                {
                    String type = scriptDef.ScriptType.ToString();
                    type = type.Substring(type.LastIndexOf(".") + 1);
                    Console.WriteLine(type + " -> " + scriptDef.ScriptResource);
                }
            }
        }
    }
}
