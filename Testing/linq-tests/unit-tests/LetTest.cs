using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using RdfMusic;

namespace UnitTests
{
	[TestFixture]
	public class TestLetClause
	{
		[Test, Ignore("WIP")]
		public void TestSimpleTestWithLet()
		{
			var ctx = new MusicDataContext("http://localhost/linqtordf/sparql");
			var q = from a in ctx.Albums
					let c = a.Tracks.Count()
					where c > 5
					select a;
			var count = q.Count();
			Assert.IsTrue(count > 0);
		}
	}
}
