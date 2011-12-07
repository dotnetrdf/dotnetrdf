using System;
using System.Collections;
using System.Text;
using System.Collections.Generic;
using VDS.RDF.Linq;
using NUnit.Framework;
using RdfMusic;
using System.Linq;

namespace UnitTests 
{
	/// <summary>
	/// Summary description for JoinTests
	/// </summary>
	[TestFixture]
	public class JoinTests : HighLevelTests
	{
		public JoinTests()
		{

		}

		[Test]
		public void Test1()
		{
			LinqTripleStore ts = CreateSparqlTripleStore();
			IRdfQuery<Track> qry = new RdfDataContext(ts).ForType<Track>(); 
	        var q = from t in qry
				where t.Year == "2007" &&
				t.GenreName == "Rory Blyth: The Smartest Man in the World" 
				select new {t.Title, t.FileLocation};
			foreach(var track in q){
				Console.WriteLine(track.Title + ": " + track.FileLocation);
			}        
		}

	}
}
