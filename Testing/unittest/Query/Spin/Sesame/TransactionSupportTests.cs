using System;
using NUnit.Framework;
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
using VDS.RDF.Configuration;

namespace SpinTest.Sesame
{
 
    [TestFixture]
    public class TransactionSupportTests
        : BaseTransactionSupportTests
    {
        public static string CONFIGURATION_URI = "dotnetrdf:/spin/sesame";

        protected override bool IsConfigured()
        {
            if (!TestConfigManager.GetSettingAsBoolean(TestConfigManager.UseSesame))
            {
                Assert.Inconclusive("Test Config marks Sesame as unavailable, cannot run this test");
                return false;
            }
            return true;
        }

        private static IUpdateableStorage _storage;
        private static SpinStorageProvider _spinProvider;

        [SetUp]
        public void Init()
        {
            IsConfigured();
            Graph configuration = new Graph();
            FileLoader.Load(configuration, "resources\\spin-config.ttl");
            ConfigurationLoader.AutoConfigureObjectFactories(configuration);

            _spinProvider = (SpinStorageProvider)ConfigurationLoader.LoadObject(configuration, RDFHelper.CreateUriNode(UriFactory.Create(CONFIGURATION_URI)));
            INode storageNode = configuration.GetTriplesWithSubjectPredicate(_spinProvider.Resource, RDFHelper.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyStorageProvider))).Select (t => t.Object).First();
            _storage = (IUpdateableStorage)ConfigurationLoader.LoadObject(configuration, storageNode);
        }

        protected override IUpdateableStorage PhysicalStorage
        {
            get
            {
                return _storage;
            }
        }

        protected override SpinStorageProvider SpinProvider
        {
            get
            {
                return _spinProvider;
            }
        }

    }
}
