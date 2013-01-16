using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Datasets;

namespace VDS.RDF.Query.Medusa
{
    [TestClass]
    public class MedusaQueryEngineTests
    {
        [TestMethod]
        public void SparqlMedusaAsk1()
        {
            ISparqlAlgebra ask = new Ask(new Bgp());
            MedusaQueryProcessor processor = new MedusaQueryProcessor(new InMemoryDataset());

            IEnumerable<ISet> sets = processor.ProcessAsk((Ask)ask, null);
            Assert.IsTrue(sets.Any());
        }
    }
}
