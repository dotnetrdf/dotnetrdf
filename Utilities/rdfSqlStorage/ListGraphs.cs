using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.Data.Sql.Clients.Cmd
{
    public class ListGraphs
        : BaseConnectAction
    {
        public ListGraphs()
            : base("listgraphs", "Lists the Graphs present in the store") { }

        protected override void Run<TConn, TCommand, TParameter, TAdaptor, TException>(BaseAdoStore<TConn, TCommand, TParameter, TAdaptor, TException> manager)
        {
            try
            {
                int i = 0;
                Console.WriteLine("rdfSqlStorage: Listing Graphs...");
                foreach (Uri u in manager.ListGraphs())
                {
                    if (u != null)
                    {
                        Console.WriteLine(u.ToString());
                    }
                    else
                    {
                        Console.WriteLine("Default Graph");
                    }
                    i++;
                }
                Console.WriteLine("rdfSqlStorage: Listed " + i + " Graph(s)");
            }
            finally
            {
                manager.Dispose();
            }
        }
    }
}
