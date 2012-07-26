using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using VDS.RDF;
using VDS.RDF.Storage;

namespace AllegroGraphIndexer
{
    class Program
    {
        static void Main(string[] args)
        {
            String server, catalog, repository;
            char op;

            Console.WriteLine("AllegroGraph Indexer Utility");
            Console.WriteLine("----------------------------");

            Console.Write("Server URI: ");
            server = Console.ReadLine();
            if (server.Equals(String.Empty))
            {
                Console.WriteLine("Aborted - you must enter a valid Server URI");
                return;
            }

            Console.Write("Catalog: ");
            catalog = Console.ReadLine();
            if (catalog.Equals(String.Empty))
            {
                Console.WriteLine("Aborted - you must enter a valid Catalog");
                return;
            }

            Console.Write("Repository: ");
            repository = Console.ReadLine();
            if (repository.Equals(String.Empty))
            {
                Console.WriteLine("Aborted - you must enter a valid Repository");
                return;
            }

            Console.WriteLine("Which operation would you like to perform?");
            Console.WriteLine("i - Index Store (combine indices)");
            Console.WriteLine("I - Index Store (do not combine indices)");
            Console.WriteLine("d - Delete Store");
            op = (char)Console.Read();
            if (op != 'i' && op != 'd' && op != 'I')
            {
                Console.WriteLine("Aborted - not a valid operation");
                return;
            }

            Console.WriteLine();
            Console.Write("Are you sure you want to proceed, this operation may take a very long time? [y/n]");
            ConsoleKeyInfo key = Console.ReadKey();
            Console.WriteLine();
            if (key.KeyChar != 'y' && key.KeyChar != 'Y')
            {
                Console.WriteLine("Aborted - user aborted");
                return;
            }
            Console.WriteLine("Making the request - this may take a very long time...");
            
            try
            {
                AllegroGraphConnector agraph = new AllegroGraphConnector(server, catalog, repository);
                DateTime start = DateTime.Now;
                switch (op)
                {
                    case 'i':
                        agraph.IndexStore(true);
                        break;
                    case 'I':
                        agraph.IndexStore(false);
                        break;
                    case 'd':
                        agraph.DeleteStore(repository);
                        break;
                }
                DateTime end = DateTime.Now;

                Console.WriteLine("Operation Completed - took " + (end - start).ToString());
            }
            catch (RdfStorageException storeEx)
            {
                Console.WriteLine("Error occurred connecting to AllegroGraph:");
                Console.WriteLine(storeEx.Message);
            }
            catch (WebException webEx)
            {
                Console.WriteLine("Error occurred during the Indexing request:");
                Console.WriteLine(webEx.Message);
            }
        }
    }
}
