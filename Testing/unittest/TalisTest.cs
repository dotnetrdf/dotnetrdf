using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Storage;
using VDS.RDF.Query;
using VDS.RDF.Parsing;

namespace VDS.RDF.Test
{
    [TestClass]
    public class TalisTest : BaseTest
    {
        [TestMethod]
        public void TalisStoreQuery()
        {
            try
            {
                //Get the Talis Connection
                TalisPlatformConnector talis = new TalisPlatformConnector("rvesse-dev1", "rvesse", "4kn478wj");
                Assert.IsNotNull(talis);
                
                //Create a Talis Triple Store
                TalisTripleStore store = new TalisTripleStore(talis);

                //Try a SELECT *
                String selectAll = "SELECT * {?s ?p ?o}";
                Object results = store.ExecuteQuery(selectAll);
                Assert.IsNotNull(results, "Expected some kind of results from the Query");
                TestTools.ShowResults(results);
                Console.WriteLine("SELECT query OK");
                Console.WriteLine();

                //Try a DESCRIBE
                String describe = "DESCRIBE <http://example.org/vehicles/FordFiesta>";
                results = store.ExecuteQuery(describe);
                Assert.IsNotNull(results, "Expected some kind of results from the Query");
                TestTools.ShowResults(results);
                Console.WriteLine("DESCRIBE query OK");
                Console.WriteLine();

                //Try an ASK
                String ask = "ASK {?s ?p ?o}";
                results = store.ExecuteQuery(ask);
                Assert.IsNotNull(results, "Expected some kind of results from the Query");
                TestTools.ShowResults(results);
                Console.WriteLine("ASK query OK");
                Console.WriteLine();

                //Try another ASK
                ask = "ASK {?s <http://example.org/nosuchthing> ?o}";
                results = store.ExecuteQuery(ask);
                Assert.IsNotNull(results, "Expected some kind of results from the Query");
                TestTools.ShowResults(results);
                Console.WriteLine("ASK query OK");
                Console.WriteLine();

            }
            catch (TalisException talisEx)
            {
                TestTools.ReportError("Talis Error", talisEx, true);
            }
            catch (RdfParseException parseEx)
            {
                TestTools.ReportError("Parsing Error", parseEx, true);
            }
            catch (Exception ex)
            {
                TestTools.ReportError("Other Error", ex, true);
            }
        }

    }
}
