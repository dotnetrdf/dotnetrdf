using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VDS.RDF.Test.LinkedData.Kasabi
{
    [TestClass]
    public class NasaKasabiTests
        : BaseKasabiTests
    {
        protected override string GetDatasetID()
        {
            return "nasa";
        }

        protected override Uri GetLookupUri()
        {
            return new Uri("http://data.kasabi.com/dataset/nasa/mission/apollo-11");
        }
    }
}
