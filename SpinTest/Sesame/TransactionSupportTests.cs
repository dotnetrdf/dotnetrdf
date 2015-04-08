#define CLEANUP
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Query.Spin;
using VDS.RDF.Query.Spin.SparqlStrategies;
using VDS.RDF.Query;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Patterns;
using VDS.RDF;
using VDS.RDF.Storage;
using System.Diagnostics;
using System.Linq;
using VDS.RDF.Query.Spin.Utility;
using System.Collections.Generic;

namespace SpinTest.Sesame
{
    // TODO refactor these into "real" better tests
    [TestClass]
    public class TransactionSupportTests
        : BaseTransactionSupportTests
    {

        public static SesameHttpProtocolConnector _storage = new SesameHttpProtocolConnector("http://192.168.2.111:8080/openrdf-sesame", "TESTS");

        public TransactionSupportTests()
            : base()
        {
        }

        [ClassInitialize()]
        public static void Initialize(TestContext context)
        {
            CleanupBeforeTest(TransactionSupportTests._storage);
        }

        protected override IQueryableStorage Storage
        {
            get
            {
                return _storage;
            }
        }

    }
}
