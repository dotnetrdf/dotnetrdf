using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using VDS.RDF;
using VDS.RDF.Storage;

namespace dotNetRDFTest
{
    public class StoreHashCodeChecker
    {
        public static void Main(String[] args)
        {
            try
            {
                String server = "localhost";
                Console.WriteLine("Enter Database Server: ");
                server = Console.ReadLine();
                if (server.Equals(String.Empty)) server = "localhost";
                String db = "bbcdemo";
                Console.WriteLine("Enter Database to check: ");
                db = Console.ReadLine();
                if (db.Equals(String.Empty)) db = "bbcdemo";

                Console.WriteLine("Checking Node Hash Codes for Database '" + db + "' on Server '" + server + "'");

                MicrosoftSqlStoreManager manager = new MicrosoftSqlStoreManager(server, db, "example", "password");

                DataTable nodeData = manager.ExecuteQuery("SELECT nodeID, nodeHash FROM NODES");
                Graph g = new Graph();

                int valid = 0;
                int invalid = 0;
                foreach (DataRow r in nodeData.Rows)
                {
                    INode n = manager.LoadNode(g, r["nodeID"].ToString());
                    int hash = Int32.Parse(r["nodeHash"].ToString());

                    if (hash == n.GetHashCode())
                    {
                        valid++;
                    }
                    else
                    {
                        invalid++;
                    }
                }

                manager.Dispose();

                Console.WriteLine(valid + " Nodes had Valid Hash Codes");
                Console.WriteLine(invalid + " Nodes had Invalid Hash Codes");

                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Exception innerEx = ex.InnerException;
                while (innerEx != null)
                {
                    Console.WriteLine();
                    Console.WriteLine(innerEx.Message);
                    Console.WriteLine(innerEx.StackTrace);
                    innerEx = innerEx.InnerException;
                }

                Console.ReadLine();
            }
        }
    }
}
