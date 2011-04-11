using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using dotSesame = org.openrdf.model;
using dotSesameRepo = org.openrdf.repository;
using VDS.RDF.Interop.Sesame;
using VDS.RDF.Storage;

namespace sesame_test
{
    [TestClass]
    public class SesameInteropTests
    {
        [TestMethod]
        public void InteropSesameContextsForVirtuoso()
        {
            VirtuosoManager virtuoso = new VirtuosoManager("DB", "dba", "dba");

            DotNetRdfGenericRepository repo = new DotNetRdfGenericRepository(virtuoso);

            dotSesameRepo.RepositoryConnection conn = repo.getConnection();

            dotSesameRepo.RepositoryResult result = conn.getContextIDs();
            while (result.hasNext())
            {
                dotSesame.Resource r = result.next() as dotSesame.Resource;
                Assert.IsTrue(r != null, "Should not be null");
                Console.WriteLine(r.ToString());
            }
        }

        [TestMethod]
        public void InteropSesameLoadFromVirtuoso()
        {
            VirtuosoManager virtuoso = new VirtuosoManager("DB", "dba", "dba");

            DotNetRdfGenericRepository repo = new DotNetRdfGenericRepository(virtuoso);

            dotSesameRepo.RepositoryConnection conn = repo.getConnection();

            dotSesame.Resource[] contexts = new dotSesame.Resource[] { repo.getValueFactory().createURI("http://localhost/TurtleImportTest") };

            dotSesameRepo.RepositoryResult result = conn.getStatements(null, null, null, true, contexts);
            while (result.hasNext())
            {
                dotSesame.Statement r = result.next() as dotSesame.Statement;
                Assert.IsTrue(r != null, "Should not be null");
                Console.WriteLine(r.ToString());
            }
        }
    }
}
