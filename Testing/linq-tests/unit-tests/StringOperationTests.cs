using System.Collections;
using System.Text;
using System.Collections.Generic;
using VDS.RDF.Linq;
using log4net.Config;
using NUnit.Framework;
using RdfMusic;
using System.Linq;

namespace UnitTests
{
	[TestFixture]
	public class StringOperationTests : HighLevelTests
	{

		[SetUp]
		public void SetUp()
		{
			XmlConfigurator.Configure();
		}
  
		[Test]
		public void TestCompare()
		{
			LinqTripleStore ts = CreateSparqlTripleStore();
			IRdfQuery<Album> qry = new RdfDataContext(ts).ForType<Album>();
			var q = from a in qry where a.Name.Contains("Thomas") select a;
			List<Album> al = new List<Album>(q);
			Assert.AreEqual(1, al.Count);
			Assert.AreEqual("Thomas Laqueur - History Lectures", al[0].Name);
		}

		[Test]
		public void TestStartsWith()
		{
			LinqTripleStore ts = CreateSparqlTripleStore();
			IRdfQuery<Album> qry = new RdfDataContext(ts).ForType<Album>();
			var q = from a in qry where a.Name.StartsWith("Thomas") select a;
			List<Album> al = new List<Album>(q);
			Assert.IsTrue(al.Count == 1);
			Assert.IsTrue(al[0].Name == "Thomas Laqueur - History Lectures");
		}
		[Test]
		public void TestEndsWith()
		{
			LinqTripleStore ts = CreateSparqlTripleStore();
			IRdfQuery<Album> qry = new RdfDataContext(ts).ForType<Album>();
			var q = from a in qry where a.Name.EndsWith("podcasts") select a;
			List<Album> al = new List<Album>(q);
			Assert.IsTrue(al.Count == 1);
			Assert.IsTrue(al[0].Name == "Rory Blyth - podcasts");
		}
	}
}