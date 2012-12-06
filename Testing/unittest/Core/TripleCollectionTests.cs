using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VDS.RDF.Core
{
    [TestClass]
    public class TripleCollectionTests
    {
        [TestMethod]
        public void TripleCollectionInstantiation1()
        {
            TreeIndexedTripleCollection collection = new TreeIndexedTripleCollection();
        }
    }
}
