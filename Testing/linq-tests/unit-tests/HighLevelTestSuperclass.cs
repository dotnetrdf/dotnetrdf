using System;
using System.Reflection;
using VDS.RDF.Linq;
using VDS.RDF;
using VDS.RDF.Parsing;
using UnitTests.Properties;
using System.IO;

namespace UnitTests
{
	public class HighLevelTests
	{
        protected static IInMemoryQueryableStore CreateMemoryStore()
		{
            string musicOntology = Settings.Default.testStoreLocation;
            TripleStore store = new TripleStore();
            Graph g = new Graph();
            FileLoader.Load(g, musicOntology);
			store.Add(g);
            return store;
		}

		protected LinqTripleStore CreateSparqlTripleStore()
		{
            return new LinqTripleStore(CreateMemoryStore());
		}

		protected LinqTripleStore CreateOnlineTripleStore()
		{
			LinqTripleStore ts = new LinqTripleStore(Properties.Settings.Default.testSparqlEndpoint);
			return ts;
		}

		protected MethodInfo propertyof(Type t, string arg)
		{
			return t.GetProperty(arg).GetGetMethod();
		}

		protected MethodInfo methodof(Type t, string arg)
		{
			return GetType().GetMethod(arg);
		}

	}
}