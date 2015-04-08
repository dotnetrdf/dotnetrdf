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

namespace SpinTest.InMemory
{
    // TODO refactor these into "real" better tests
    [TestClass]
    public class TransactionSupportTests
        : BaseTransactionSupportTests
    {

        private static InMemoryManager _storage = new InMemoryManager(new TripleStore());

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
            get {
                return _storage;
            }
        }

    }
}
