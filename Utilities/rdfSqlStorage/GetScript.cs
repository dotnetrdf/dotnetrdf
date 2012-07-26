using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.Data.Sql.Clients.Cmd
{
    public class GetScript
        : BaseManagementAction
    {
        public GetScript()
            : base("getscript", "Extracts a database schema script") { }

        public override int MinimumArguments
        {
            get 
            {
                return 2; 
            }
        }

        public override void ShowUsage()
        {
            Console.WriteLine("rdfSqlStorage (getscript mode)");
            Console.WriteLine("==============================");
            Console.WriteLine();
            Console.WriteLine("Extracts a database schema script from the Data.Sql library");
            Console.WriteLine();
            Console.WriteLine("Usage is rdfSqlStorage getscript dbtype schema script");
            Console.WriteLine();
            Console.WriteLine("Supported Database Types are as follows: sql, azure");
            Console.WriteLine("Use the listschemas mode to get available built-in schemas");
            Console.WriteLine("Support Scripts are as follows: create, drop");
            Console.WriteLine();
            Console.WriteLine("You may find it useful to use the listscripts mode to get available scripts for a schema though this will not guarantee that a given script is available for a specific database type");
        }

        public override void Run(string[] args)
        {
            if (args.Length < 4)
            {
                this.ShowUsage();
                return;
            }
            if (args[1].Equals("-help"))
            {
                this.ShowUsage();
                return;
            }

            //Get the arguments
            AdoSchemaScriptDatabase db = AdoSchemaScriptDatabase.MicrosoftSqlServer;
            switch (args[1].ToLower())
            {
                case "sql":
                case "azure":
                    db = AdoSchemaScriptDatabase.MicrosoftSqlServer;
                    break;
                default:
                    Console.Error.WriteLine("rdfSqlStorage: Error: '" + args[1] + "' is not a supported database type, supported types are: sql, azure");
                    return;
            }
            String schema = args[2];
            AdoSchemaDefinition def = AdoSchemaHelper.SchemaDefinitions.Where(d => d.Name.Equals(schema)).FirstOrDefault();
            if (def == null)
            {
                Console.Error.WriteLine("rdfSqlStorage: Error: There is no built-in schema named '" + schema + "', use the listschemas mode to view available built-in schemas");
                return;
            }
            else
            {
                AdoSchemaScriptType scriptType = AdoSchemaScriptType.Create;
                switch (args[3].ToLower())
                {
                    case "create":
                        scriptType = AdoSchemaScriptType.Create;
                        break;
                    case "drop":
                        scriptType = AdoSchemaScriptType.Drop;
                        break;
                    default:
                        Console.Error.WriteLine("rdfSqlStorage: Error: '" + args[3] + "' is not a supported script, supported values are: Create, Drop");
                        return;
                }

                if (def.HasScript(scriptType, db))
                {
                    String scriptResource = def.GetScript(scriptType, db);
                    if (scriptResource != null)
                    {
                        Stream stream = Assembly.GetAssembly(typeof(AdoSchemaDefinition)).GetManifestResourceStream(scriptResource);
                        if (stream != null)
                        {
                            using (StreamReader reader = new StreamReader(stream))
                            {
                                Console.WriteLine(reader.ReadToEnd());
                                reader.Close();
                            }
                        }
                        else
                        {
                            Console.Error.WriteLine("rdfSqlStorage: Error: Schema '" + schema + "' has a " + args[3] + " available for database type " + args[1] + " but it the script resource could not be extracted from the DLL");
                            return;
                        }
                    }
                    else
                    {
                        Console.Error.WriteLine("rdfSqlStorage: Error: Schema '" + schema + "' does not have a " + args[3] + " available for database type " + args[1]);
                        return;
                    }
                }
                else
                {
                    Console.Error.WriteLine("rdfSqlStorage: Error: Schema '" + schema + "' does not have a " + args[3] + " available for database type " + args[1]);
                    return;
                }
            }
        }
    }
}
