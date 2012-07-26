using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.Data.Sql.Clients.Cmd
{
    public class Drop
        : BaseConnectAction
    {
        public Drop()
            : base("drop", "Drops the ADO Store if it exists, note that you may see some errors here as this does not remove users from the database roles associated with the ADO Store") { }

        protected override void  Run<TConn,TCommand,TParameter,TAdaptor,TException>(BaseAdoStore<TConn,TCommand,TParameter,TAdaptor,TException> manager)
        {
            try
            {
                String schema = manager.CheckSchema();
                AdoSchemaDefinition def = AdoSchemaHelper.SchemaDefinitions.Where(d => d.Name.Equals(schema)).FirstOrDefault();
                if (def != null)
                {
                    String script = def.GetScript(AdoSchemaScriptType.Drop, AdoSchemaScriptDatabase.MicrosoftSqlServer);
                    if (script != null)
                    {
                        manager.ExecuteSqlFromResource(script);
                    }
                    else
                    {
                        Console.Error.WriteLine("rdfSqlStorage: Error: Schema " + schema + " does not have a drop script associated with it so cannot be dropped with this tool");
                    }
                }
                else
                {
                    Console.Error.WriteLine("rdfSqlStorage: Error: Schema " + schema + " is not a built-in schema so cannot be created/dropped with this tool");
                }
            }
            finally
            {
                manager.Dispose();
            }
        }
    }
}
